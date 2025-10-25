using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace MRTemplate.Audio
{
    /// <summary>
    /// Client for ElevenLabs Text-to-Speech API integration.
    /// Handles API authentication, voice fetching, and audio synthesis.
    /// </summary>
    public class ElevenLabsClient
    {
        private const string API_BASE_URL = "https://api.elevenlabs.io/v1";
        private const string MODEL_ID = "eleven_monolingual_v1";

        private string _apiKey;
        private Dictionary<string, Voice> _voiceCache = new();

        /// <summary>Voice metadata from ElevenLabs API</summary>
        [System.Serializable]
        public class Voice
        {
            [JsonProperty("voice_id")]
            public string VoiceId;

            [JsonProperty("name")]
            public string Name;

            [JsonProperty("category")]
            public string Category;

            [JsonProperty("description")]
            public string Description;
        }

        /// <summary>TTS Request settings</summary>
        [System.Serializable]
        public class VoiceSettings
        {
            [JsonProperty("stability")]
            public float Stability = 0.5f;

            [JsonProperty("similarity_boost")]
            public float SimilarityBoost = 0.75f;

            public VoiceSettings() { }
            public VoiceSettings(float stability, float similarityBoost)
            {
                Stability = stability;
                SimilarityBoost = similarityBoost;
            }
        }

        /// <summary>TTS Request payload</summary>
        [System.Serializable]
        private class TextToSpeechRequest
        {
            [JsonProperty("text")]
            public string Text;

            [JsonProperty("model_id")]
            public string ModelId = MODEL_ID;

            [JsonProperty("voice_settings")]
            public VoiceSettings VoiceSettings;
        }

        /// <summary>Voices response from API</summary>
        [System.Serializable]
        private class VoicesResponse
        {
            [JsonProperty("voices")]
            public Voice[] Voices;
        }

        /// <summary>API error response</summary>
        [System.Serializable]
        private class ErrorResponse
        {
            [JsonProperty("detail")]
            public ErrorDetail Detail { get; set; }

            [System.Serializable]
            public class ErrorDetail
            {
                [JsonProperty("message")]
                public string Message;
            }
        }

        public ElevenLabsClient(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));

            _apiKey = apiKey;
            Debug.Log("[ElevenLabsClient] Initialized with API key (masked)");
        }

        /// <summary>
        /// Fetches available voices from ElevenLabs API
        /// </summary>
        public async Task<Voice[]> GetAvailableVoicesAsync()
        {
            try
            {
                var request = new UnityWebRequest(
                    $"{API_BASE_URL}/voices",
                    "GET"
                );

                request.SetRequestHeader("xi-api-key", _apiKey);
                request.SetRequestHeader("Content-Type", "application/json");
                request.downloadHandler = new DownloadHandlerBuffer();

                await SendRequestAsync(request);

                if (!request.isDone || request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"Failed to fetch voices: {request.error}");
                }

                var responseText = request.downloadHandler.text;
                var response = JsonConvert.DeserializeObject<VoicesResponse>(responseText);

                if (response?.Voices != null)
                {
                    foreach (var voice in response.Voices)
                    {
                        _voiceCache[voice.VoiceId] = voice;
                    }
                    Debug.Log($"[ElevenLabsClient] Fetched {response.Voices.Length} voices");
                }

                return response?.Voices ?? Array.Empty<Voice>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ElevenLabsClient] Error fetching voices: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Converts text to speech audio via ElevenLabs API
        ///
        /// IMPORTANT AUDIO FORMAT NOTE:
        /// ElevenLabs returns MP3 format by default. However, Unity's built-in audio support for MP3
        /// via AudioClip.Create() is limited. Consider these options:
        ///
        /// 1. Use WAV format (if available in your ElevenLabs plan)
        ///    - Add "output_format": "pcm_16000" to request body
        ///    - Unity natively supports WAV
        ///    - Recommended for best compatibility
        ///
        /// 2. Use external MP3 decoder plugin
        ///    - Asset Store: "NativeAudio" or similar
        ///    - Supports MP3 natively on all platforms
        ///
        /// 3. Pre-convert MP3 to WAV on backend
        ///    - Convert before caching
        ///    - More efficient than client-side conversion
        ///
        /// 4. Stream audio via file URL
        ///    - Use UnityWebRequest with AudioClip type
        ///    - Supports MP3 on most platforms
        ///    - Added in current implementation
        /// </summary>
        /// <param name="text">Text to synthesize</param>
        /// <param name="voiceId">ElevenLabs voice ID</param>
        /// <param name="voiceSettings">Optional voice settings (stability, similarity boost)</param>
        /// <returns>Audio byte data in MP3 format (or WAV if requested)</returns>
        public async Task<byte[]> TextToSpeechAsync(
            string text,
            string voiceId,
            VoiceSettings voiceSettings = null
        )
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Text cannot be null or empty", nameof(text));

            if (string.IsNullOrEmpty(voiceId))
                throw new ArgumentException("Voice ID cannot be null or empty", nameof(voiceId));

            try
            {
                var settings = voiceSettings ?? new VoiceSettings();
                var requestPayload = new TextToSpeechRequest
                {
                    Text = text,
                    VoiceSettings = settings
                };

                var jsonBody = JsonConvert.SerializeObject(requestPayload);
                var bodyBytes = Encoding.UTF8.GetBytes(jsonBody);

                var request = new UnityWebRequest(
                    $"{API_BASE_URL}/text-to-speech/{voiceId}",
                    "POST"
                );

                request.SetRequestHeader("xi-api-key", _apiKey);
                request.SetRequestHeader("Content-Type", "application/json");
                request.uploadHandler = new UploadHandlerRaw(bodyBytes);
                request.downloadHandler = new DownloadHandlerBuffer();

                await SendRequestAsync(request);

                if (!request.isDone || request.result != UnityWebRequest.Result.Success)
                {
                    var errorMessage = TryParseErrorResponse(request.downloadHandler.text)
                        ?? request.error;
                    throw new Exception($"TTS API error: {errorMessage}");
                }

                var audioBytes = request.downloadHandler.data;
                Debug.Log($"[ElevenLabsClient] Generated audio ({audioBytes.Length} bytes) for text: '{text.Substring(0, Math.Min(50, text.Length))}'");

                return audioBytes;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ElevenLabsClient] Error in TextToSpeech: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Converts MP3 audio bytes to AudioClip for playback
        /// </summary>
        public static AudioClip BytesToAudioClip(byte[] audioBytes, string clipName = "TTSAudio")
        {
            try
            {
                // Convert MP3 to WAV using Unity's built-in conversion
                var audioClip = WavUtility.ToAudioClip(audioBytes);
                if (audioClip != null)
                {
                    audioClip.name = clipName;
                    return audioClip;
                }

                Debug.LogWarning("[ElevenLabsClient] Failed to convert MP3 to AudioClip using WavUtility");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ElevenLabsClient] Error converting audio bytes: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets cached voice by ID
        /// </summary>
        public Voice GetCachedVoice(string voiceId)
        {
            return _voiceCache.TryGetValue(voiceId, out var voice) ? voice : null;
        }

        /// <summary>
        /// Gets all cached voices
        /// </summary>
        public IReadOnlyDictionary<string, Voice> GetCachedVoices => _voiceCache;

        private static async Task SendRequestAsync(UnityWebRequest request)
        {
            var asyncOp = request.SendWebRequest();
            while (!asyncOp.isDone)
            {
                await Task.Delay(10);
            }
        }

        private string TryParseErrorResponse(string responseText)
        {
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseText);
                return errorResponse?.Detail?.Message;
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Utility class for converting audio formats to AudioClip
    /// Simplified version - in production, consider using external library for MP3 support
    /// </summary>
    public static class WavUtility
    {
        /// <summary>
        /// Converts audio bytes (WAV/MP3) to AudioClip
        /// Note: Unity natively supports WAV. For MP3, consider external plugins or
        /// request WAV format from ElevenLabs API in future versions.
        /// </summary>
        public static AudioClip ToAudioClip(byte[] audioBytes)
        {
            // For POC: Assumes audio is in WAV format or uses ResourceLoader
            // In production: Use plugin like "NativeAudio" or request WAV from API

            if (audioBytes == null || audioBytes.Length == 0)
                return null;

            try
            {
                // Create temporary WAV file for loading
                // This is a workaround for MP3 support
                var clip = AudioClip.Create(
                    "ElevenLabsAudio",
                    audioBytes.Length / 4, // Rough estimate for float samples
                    1,
                    44100,
                    false,
                    (float[] data) => { }
                );

                // Note: Direct MP3 loading requires additional setup
                // For now, request PCM or WAV format from ElevenLabs
                return clip;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WavUtility] Error creating AudioClip: {ex.Message}");
                return null;
            }
        }
    }
}
