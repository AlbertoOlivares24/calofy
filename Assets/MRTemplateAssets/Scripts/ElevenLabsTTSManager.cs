using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace MRTemplate.Audio
{
    /// <summary>
    /// Manages ElevenLabs text-to-speech functionality for the MR application.
    /// Handles API requests, audio caching, playback, and lifecycle management.
    /// Follows the Manager pattern established in the codebase (e.g., ARFeatureController, OcclusionManager).
    /// </summary>
    public class ElevenLabsTTSManager : MonoBehaviour
    {
        [SerializeField]
        private ElevenLabsConfig _config;

        [SerializeField]
        [Tooltip("AudioSource for playing TTS audio")]
        private AudioSource _audioSource;

        [SerializeField]
        [Tooltip("Log verbose debug messages")]
        private bool _debugLogging = true;

        private ElevenLabsClient _client;
        private Queue<TTSRequest> _requestQueue = new();
        private string _cacheDirectoryPath;
        private bool _isProcessing = false;
        private ElevenLabsClient.Voice[] _availableVoices;

        /// <summary>TTS request queued for processing</summary>
        public class TTSRequest
        {
            public string Text;
            public string VoiceId;
            public ElevenLabsClient.VoiceSettings VoiceSettings;
            public Action<AudioClip> OnSuccess;
            public Action<string> OnError;
            public bool UseCache = true;
        }

        /// <summary>Fired when TTS audio starts playing</summary>
        public event Action OnPlaybackStarted;

        /// <summary>Fired when TTS audio stops playing</summary>
        public event Action OnPlaybackEnded;

        /// <summary>Fired when TTS generation fails</summary>
        public event Action<string> OnError;

        private void OnEnable()
        {
            if (_config == null)
            {
                Debug.LogError("[ElevenLabsTTSManager] Config is not assigned");
                enabled = false;
                return;
            }

            if (!_config.IsValid())
            {
                Debug.LogError("[ElevenLabsTTSManager] Config is invalid");
                enabled = false;
                return;
            }

            InitializeManager();
        }

        private void OnDisable()
        {
            StopAllAudio();
        }

        private async void InitializeManager()
        {
            try
            {
                _client = new ElevenLabsClient(_config.ApiKey);
                _cacheDirectoryPath = Path.Combine(
                    Application.persistentDataPath,
                    _config.CacheDirectory
                );

                if (_config.EnableCaching && !Directory.Exists(_cacheDirectoryPath))
                {
                    Directory.CreateDirectory(_cacheDirectoryPath);
                    Log($"Created cache directory: {_cacheDirectoryPath}");
                }

                // Fetch available voices on startup
                await FetchAvailableVoicesAsync();

                Log("ElevenLabsTTSManager initialized successfully");
            }
            catch (Exception ex)
            {
                LogError($"Failed to initialize: {ex.Message}");
                OnError?.Invoke($"Initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Queues a TTS request for processing
        /// </summary>
        public void QueueTTSRequest(
            string text,
            string voiceId,
            Action<AudioClip> onSuccess,
            Action<string> onError,
            ElevenLabsClient.VoiceSettings voiceSettings = null,
            bool useCache = true
        )
        {
            if (!gameObject.activeInHierarchy || !enabled)
            {
                onError?.Invoke("ElevenLabsTTSManager is not active");
                return;
            }

            var request = new TTSRequest
            {
                Text = text,
                VoiceId = string.IsNullOrEmpty(voiceId) ? _config.DefaultVoiceId : voiceId,
                VoiceSettings = voiceSettings ?? new ElevenLabsClient.VoiceSettings(
                    _config.VoiceStability,
                    _config.SimilarityBoost
                ),
                OnSuccess = onSuccess,
                OnError = onError,
                UseCache = useCache && _config.EnableCaching
            };

            _requestQueue.Enqueue(request);
            Log($"Queued TTS request: '{text.Substring(0, Math.Min(50, text.Length))}'");

            if (!_isProcessing)
            {
                ProcessNextRequest();
            }
        }

        /// <summary>
        /// Plays audio immediately (for simple use cases)
        /// </summary>
        public async Task<bool> SpeakAsync(string text, string voiceId = null)
        {
            try
            {
                var actualVoiceId = string.IsNullOrEmpty(voiceId) ? _config.DefaultVoiceId : voiceId;

                var audioBytes = await GetOrGenerateAudioAsync(
                    text,
                    actualVoiceId,
                    new ElevenLabsClient.VoiceSettings(
                        _config.VoiceStability,
                        _config.SimilarityBoost
                    )
                );

                if (audioBytes == null || audioBytes.Length == 0)
                {
                    LogError("Failed to generate audio bytes");
                    return false;
                }

                var audioClip = CreateAudioClipFromBytes(audioBytes);
                if (audioClip == null)
                {
                    LogError("Failed to create AudioClip from bytes");
                    return false;
                }

                PlayAudioClip(audioClip);
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error in SpeakAsync: {ex.Message}");
                OnError?.Invoke($"Speech generation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Stops all audio playback
        /// </summary>
        public void StopAllAudio()
        {
            if (_audioSource != null && _audioSource.isPlaying)
            {
                _audioSource.Stop();
                OnPlaybackEnded?.Invoke();
            }
        }

        /// <summary>
        /// Pauses current audio playback
        /// </summary>
        public void PauseAudio()
        {
            if (_audioSource != null && _audioSource.isPlaying)
            {
                _audioSource.Pause();
            }
        }

        /// <summary>
        /// Resumes paused audio playback
        /// </summary>
        public void ResumeAudio()
        {
            if (_audioSource != null && !_audioSource.isPlaying)
            {
                _audioSource.Play();
                OnPlaybackStarted?.Invoke();
            }
        }

        /// <summary>
        /// Gets available voices (cached after first fetch)
        /// </summary>
        public ElevenLabsClient.Voice[] GetAvailableVoices() => _availableVoices ?? Array.Empty<ElevenLabsClient.Voice>();

        /// <summary>
        /// Clears the audio cache
        /// </summary>
        public void ClearCache()
        {
            try
            {
                if (Directory.Exists(_cacheDirectoryPath))
                {
                    Directory.Delete(_cacheDirectoryPath, true);
                    Directory.CreateDirectory(_cacheDirectoryPath);
                    Log("Audio cache cleared");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error clearing cache: {ex.Message}");
            }
        }

        private async void ProcessNextRequest()
        {
            if (_isProcessing || _requestQueue.Count == 0)
                return;

            _isProcessing = true;
            var request = _requestQueue.Dequeue();

            try
            {
                var audioBytes = await GetOrGenerateAudioAsync(
                    request.Text,
                    request.VoiceId,
                    request.VoiceSettings
                );

                if (audioBytes == null)
                {
                    throw new Exception("Failed to generate audio");
                }

                var audioClip = CreateAudioClipFromBytes(audioBytes);
                if (audioClip == null)
                {
                    throw new Exception("Failed to create AudioClip");
                }

                // Play the audio automatically
                PlayAudioClip(audioClip);

                // Also invoke the success callback for the caller
                request.OnSuccess?.Invoke(audioClip);
            }
            catch (Exception ex)
            {
                var errorMsg = $"TTS request failed: {ex.Message}";
                LogError(errorMsg);
                request.OnError?.Invoke(errorMsg);
                OnError?.Invoke(errorMsg);
            }
            finally
            {
                _isProcessing = false;
                if (_requestQueue.Count > 0)
                {
                    ProcessNextRequest();
                }
            }
        }

        private async Task<byte[]> GetOrGenerateAudioAsync(
            string text,
            string voiceId,
            ElevenLabsClient.VoiceSettings voiceSettings
        )
        {
            // Check cache first
            var cacheKey = GenerateCacheKey(text, voiceId);
            var cachedAudio = LoadFromCache(cacheKey);
            if (cachedAudio != null)
            {
                Log($"Using cached audio for: '{text.Substring(0, Math.Min(30, text.Length))}'");
                return cachedAudio;
            }

            // Generate new audio
            var audioBytes = await _client.TextToSpeechAsync(text, voiceId, voiceSettings);

            // Save to cache
            if (audioBytes != null && audioBytes.Length > 0)
            {
                SaveToCache(cacheKey, audioBytes);
            }

            return audioBytes;
        }

        private async Task FetchAvailableVoicesAsync()
        {
            try
            {
                _availableVoices = await _client.GetAvailableVoicesAsync();
                Log($"Fetched {_availableVoices.Length} available voices");
            }
            catch (Exception ex)
            {
                LogError($"Failed to fetch voices: {ex.Message}");
                _availableVoices = Array.Empty<ElevenLabsClient.Voice>();
            }
        }

        /// <summary>
        /// Creates an AudioClip from MP3/WAV bytes
        ///
        /// IMPORTANT: ElevenLabs returns MP3 by default, but Unity doesn't natively support MP3 playback
        /// via AudioClip.Create(). This is a known limitation in the POC.
        ///
        /// Workarounds:
        /// 1. Request WAV format from ElevenLabs API (if available in your plan)
        /// 2. Use external MP3 decoder plugin (e.g., NativeAudio from Asset Store)
        /// 3. Pre-convert MP3 to WAV on backend before caching
        /// 4. Save bytes to file and load via WWW/UnityWebRequest
        /// </summary>
        private AudioClip CreateAudioClipFromBytes(byte[] audioBytes)
        {
            try
            {
                if (audioBytes == null || audioBytes.Length == 0)
                {
                    LogError("Audio bytes are empty");
                    return null;
                }

                // Attempt to load MP3/WAV from bytes
                // This requires either:
                // A) Custom decoder (not in POC)
                // B) Converting MP3 to WAV first (not in POC)
                // C) Using external plugin

                // For now, save to file and load via file URL
                // This works around Unity's lack of built-in MP3 support
                return LoadAudioClipFromFile(audioBytes);
            }
            catch (Exception ex)
            {
                LogError($"Error creating AudioClip: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Loads audio from bytes by temporarily saving to file
        /// This is a workaround for Unity's lack of native MP3 support
        /// </summary>
        private AudioClip LoadAudioClipFromFile(byte[] audioBytes)
        {
            try
            {
                // Create temporary file path for audio
                var tempAudioPath = Path.Combine(
                    Application.temporaryCachePath,
                    $"tts_audio_{System.Guid.NewGuid()}.mp3"
                );

                // Write bytes to file
                File.WriteAllBytes(tempAudioPath, audioBytes);

                // Load via UnityWebRequest (supports MP3 on most platforms)
                var audioClip = LoadAudioClipFromPath(tempAudioPath);

                // Clean up temp file after loading
                if (File.Exists(tempAudioPath))
                {
                    File.Delete(tempAudioPath);
                }

                return audioClip;
            }
            catch (Exception ex)
            {
                LogError($"Error loading audio from file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Loads audio clip from file path using file:// URL
        /// Supports MP3 natively on most platforms via UnityWebRequest
        /// </summary>
        private AudioClip LoadAudioClipFromPath(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    LogError($"Audio file not found: {filePath}");
                    return null;
                }

                // Convert file path to file:// URL for UnityWebRequest
                var fileUrl = "file://" + filePath;

                // Load MP3 via UnityWebRequest (supports MP3 natively)
                using (var www = UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.MPEG))
                {
                    var asyncOp = www.SendWebRequest();

                    // Wait for loading to complete (blocking call for cached audio)
                    while (!asyncOp.isDone)
                    {
                        System.Threading.Thread.Sleep(10);
                    }

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        LogError($"Failed to load audio from {filePath}: {www.error}");
                        return null;
                    }

                    var audioClip = DownloadHandlerAudioClip.GetContent(www);
                    if (audioClip == null)
                    {
                        LogError("AudioClip is null after loading");
                        return null;
                    }

                    audioClip.name = Path.GetFileNameWithoutExtension(filePath);
                    Log($"Loaded MP3 AudioClip from: {filePath} (Length: {audioClip.length:F2}s)");
                    return audioClip;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error loading audio clip from path: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Plays audio clip via AudioSource
        /// Properly configures Unity's AudioSource component
        /// </summary>
        private void PlayAudioClip(AudioClip clip)
        {
            if (clip == null)
            {
                LogError("Audio clip is null");
                return;
            }

            if (_audioSource == null)
            {
                LogError("AudioSource is not assigned. Please assign an AudioSource in the inspector.");
                return;
            }

            try
            {
                // Set the clip
                _audioSource.clip = clip;

                // Ensure AudioSource is configured properly
                _audioSource.playOnAwake = false;

                // Play the audio
                _audioSource.Play();

                // Notify listeners
                OnPlaybackStarted?.Invoke();

                Log($"Playing audio via AudioSource: {clip.name} " +
                    $"(Length: {clip.length:F2}s, Channels: {clip.channels}, " +
                    $"SampleRate: {clip.frequency}Hz, Volume: {_audioSource.volume:F2})");

                // Start coroutine to detect when playback ends
                StartCoroutine(WaitForAudioCompletion(clip.length));
            }
            catch (Exception ex)
            {
                LogError($"Error playing audio clip: {ex.Message}");
                OnError?.Invoke($"Playback error: {ex.Message}");
            }
        }

        /// <summary>
        /// Waits for audio to finish playing and fires completion event
        /// </summary>
        private IEnumerator WaitForAudioCompletion(float audioLength)
        {
            yield return new WaitForSeconds(audioLength + 0.1f); // Small buffer

            // Verify audio actually finished
            if (_audioSource != null && !_audioSource.isPlaying)
            {
                OnPlaybackEnded?.Invoke();
                Log("Audio playback completed");
            }
        }

        /// <summary>
        /// Sets the volume of TTS audio (0-1)
        /// </summary>
        public void SetVolume(float volume)
        {
            if (_audioSource == null)
            {
                LogError("AudioSource is not assigned");
                return;
            }

            volume = Mathf.Clamp01(volume);
            _audioSource.volume = volume;
            Log($"TTS volume set to {volume:F2}");
        }

        /// <summary>
        /// Gets the current volume
        /// </summary>
        public float GetVolume()
        {
            return _audioSource != null ? _audioSource.volume : 0f;
        }

        /// <summary>
        /// Enables/disables 3D audio positioning for spatial audio in VR
        /// Set to 1.0 for full 3D, 0.0 for 2D (head-relative)
        /// </summary>
        public void SetSpatialAudio(float spatialBlend = 1f)
        {
            if (_audioSource == null)
            {
                LogError("AudioSource is not assigned");
                return;
            }

            _audioSource.spatialBlend = Mathf.Clamp01(spatialBlend);
            Log($"Spatial audio blend set to {spatialBlend:F2}");
        }

        /// <summary>
        /// Sets the pitch of the audio (0.5 = half speed, 1.0 = normal, 2.0 = double speed)
        /// </summary>
        public void SetPitch(float pitch)
        {
            if (_audioSource == null)
            {
                LogError("AudioSource is not assigned");
                return;
            }

            pitch = Mathf.Max(0.1f, pitch); // Prevent invalid pitch
            _audioSource.pitch = pitch;
            Log($"TTS pitch set to {pitch:F2}");
        }

        /// <summary>
        /// Gets AudioSource for advanced control
        /// </summary>
        public AudioSource GetAudioSource() => _audioSource;

        private byte[] LoadFromCache(string cacheKey)
        {
            if (!_config.EnableCaching)
                return null;

            try
            {
                var cachePath = Path.Combine(_cacheDirectoryPath, cacheKey);
                if (File.Exists(cachePath))
                {
                    return File.ReadAllBytes(cachePath);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error loading from cache: {ex.Message}");
            }

            return null;
        }

        private void SaveToCache(string cacheKey, byte[] audioBytes)
        {
            if (!_config.EnableCaching)
                return;

            try
            {
                var cachePath = Path.Combine(_cacheDirectoryPath, cacheKey);
                File.WriteAllBytes(cachePath, audioBytes);
                Log($"Saved audio to cache: {cacheKey}");
            }
            catch (Exception ex)
            {
                LogError($"Error saving to cache: {ex.Message}");
            }
        }

        private string GenerateCacheKey(string text, string voiceId)
        {
            // Create deterministic hash of text + voiceId
            var combined = $"{text}_{voiceId}";
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                // Use BitConverter for .NET Standard 2.1 compatibility (Convert.ToHexString is .NET 5+)
                return BitConverter.ToString(hash).Replace("-", "").ToLower() + ".mp3";
            }
        }

        private void Log(string message)
        {
            if (_debugLogging)
                Debug.Log($"[ElevenLabsTTSManager] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[ElevenLabsTTSManager] {message}");
        }
    }
}
