using UnityEngine;
using UnityEngine.Networking;          // StreamingAssets read on Android
using System.Threading.Tasks;
using System.Diagnostics;             // Stopwatch
using System.IO;                      // Path / File
using Debug = UnityEngine.Debug;

namespace RG.LabBot
{
    public class RobotSpeech : MonoBehaviour
    {
        public AudioSource speaker;
        public string projectId;
        public string region = "us-central1";
        public string geminiModel = "gemini-1.5-flash";   // set to gemini-2.0-flash-001 in Inspector if preferred
        public string ttsVoice = "en-US-Neural2-J";
        public string credentialsJsonPath;

        // ---- init gate ----
        Task _initTask;
        bool _ready;

        void Awake()
        {
            // Kick off init early (Awake order is deterministic inside a scene)
            _initTask = InitAsync();
        }

        // Copies gcp_key.json from StreamingAssets to persistentDataPath on Android,
        // then configures GoogleAuth with the resolved path.
        async Task InitAsync()
        {
            // Resolve path first
            var path = await ResolveCredentialsPathAsync("starry-argon-473718-t5-c86196d8db83.json");
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Debug.LogError("[RobotSpeech] Service account JSON not found on device.");
                _ready = false;
                return;
            }

            credentialsJsonPath = path; // store for visibility
            Debug.Log($"[RobotSpeech] Using JSON at: {credentialsJsonPath}");

            // Configure Google auth
            GoogleAuth.ConfigureServiceAccount(credentialsJsonPath);
            _ready = true;
        }

        // ANDROID-SAFE CREDENTIALS RESOLVER
        async Task<string> ResolveCredentialsPathAsync(string fileName = "starry-argon-473718-t5-c86196d8db83.json")
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var src = Path.Combine(Application.streamingAssetsPath, fileName);   // jar:file:///...!/assets/starry-argon-473718-t5-c86196d8db83.json
            var dst = Path.Combine(Application.persistentDataPath, fileName);   // /storage/.../files/gcp_key.json

            if (!File.Exists(dst))
            {
                Debug.Log($"[Auth] Copying service account from APK:\n  {src}\n  -> {dst}");
                using var www = UnityWebRequest.Get(src);
                var op = www.SendWebRequest();
                while (!op.isDone) await Task.Yield();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[Auth] Failed to read StreamingAssets JSON: {www.error}");
                    return null;
                }
                File.WriteAllBytes(dst, www.downloadHandler.data);
            }
            return dst;
#else
            // Editor/desktop: use inspector path if set; otherwise default to StreamingAssets
            var path = string.IsNullOrEmpty(credentialsJsonPath)
                ? Path.Combine(Application.streamingAssetsPath, fileName)
                : credentialsJsonPath;

            if (path.StartsWith("Assets/"))
                path = Path.GetFullPath(path); // make absolute on desktop

            return path;
#endif
        }

        public async Task SpeakFromPromptAsync(string prompt)
        {
            // Ensure auth init is complete before any network calls
            if (_initTask != null) await _initTask;
            if (!_ready)
            {
                Debug.LogError("[RobotSpeech] Not ready (auth not configured). Aborting speech.");
                return;
            }

            var sw = Stopwatch.StartNew();
            try
            {
                // Debug.Log($"[RobotSpeech] ▶ Prompt: {prompt}");

                // 1) LLM
                var text = await RobotGeminiClient.GenerateAsync(projectId, region, geminiModel, prompt);
                // Debug.Log($"[RobotSpeech] ◀ Gemini text ({(text?.Length ?? 0)} chars): {text}");

                if (string.IsNullOrWhiteSpace(text))
                {
                    // Debug.LogWarning("[RobotSpeech] Gemini returned empty text.");
                    return;
                }

                // 2) TTS
                var mp3Path = await RobotTTSClient.SynthesizeToMp3FileAsync(text, ttsVoice);
                // Debug.Log($"[RobotSpeech] 💾 MP3 saved: {mp3Path}");

                // 3) Audio
                await RobotAudio.PlayMp3Async(speaker, mp3Path);
                // Debug.Log($"[RobotSpeech] 🔊 Playback requested.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[RobotSpeech] ERROR: " + ex);
            }
            finally
            {
                sw.Stop();
                // Debug.Log($"[RobotSpeech] ⏱ Total time: {sw.ElapsedMilliseconds} ms");
            }
        }
    }
}
