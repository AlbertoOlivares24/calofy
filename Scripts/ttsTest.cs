using System;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

// Google auth
using Google.Apis.Auth.OAuth2;

public class TtsTest : MonoBehaviour
{
    [Header("Optional: hook an AudioSource to hear playback")]
    public AudioSource audioSource;

    // Pick a voice you like
    private const string LanguageCode = "en-US";
    private const string VoiceName = "en-US-Neural2-C"; // change if you want

    private string Mp3Path => Path.Combine(Application.persistentDataPath, "tts_test.mp3");

    private async void Start()
    {
        // Quick demo string (replace with your own)
        await SynthesizeAndPlay("Hey team! Vertex AI TTS test from Unity is working.");
    }

    public async Task SynthesizeAndPlay(string text)
    {
        try
        {
            // 1) Load service account JSON from Resources
            var jsonKey = Resources.Load<TextAsset>("service_account"); // file must be named service_account.json
            if (jsonKey == null)
            {
                Debug.LogError("Missing Assets/Resources/service_account.json");
                return;
            }

            // 2) Get an access token (using cloud-platform scope)
            var credential = GoogleCredential.FromJson(jsonKey.text)
                .CreateScoped("https://www.googleapis.com/auth/cloud-platform");

            string token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

            // 3) Build REST request body
            var payload = new TtsRequest
            {
                input = new TtsInput { text = text },
                voice = new TtsVoice { languageCode = LanguageCode, name = VoiceName },
                audioConfig = new TtsAudioConfig { audioEncoding = "MP3" }
            };

            string jsonBody = JsonUtility.ToJson(payload);


            // 4) POST to Cloud Text-to-Speech
            const string url = "https://texttospeech.googleapis.com/v1/text:synthesize";
            using (var req = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
                req.SetRequestHeader("Authorization", $"Bearer {token}");

                var op = req.SendWebRequest();
                while (!op.isDone) await Task.Yield();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"TTS failed: {req.responseCode} {req.error}\n{req.downloadHandler.text}");
                    return;
                }

                var resp = JsonUtility.FromJson<TtsResponse>(req.downloadHandler.text);
                if (string.IsNullOrEmpty(resp.audioContent))
                {
                    Debug.LogError("No audioContent in response.");
                    return;
                }

                // 5) Save MP3
                byte[] mp3 = Convert.FromBase64String(resp.audioContent);
                File.WriteAllBytes(Mp3Path, mp3);
                Debug.Log($"Saved: {Mp3Path}");

                // 6) Play MP3
                string uri = "file://" + Mp3Path.Replace("\\", "/");
                using (var get = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG))
                {
                    var op2 = get.SendWebRequest();
                    while (!op2.isDone) await Task.Yield();

                    if (get.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Load MP3 error: {get.error}");
                        return;
                    }

                    var clip = DownloadHandlerAudioClip.GetContent(get);
                    if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.clip = clip;
                    audioSource.Play();
                    Debug.Log("Playing speech.");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("TTS demo error: " + ex);
        }
    }

    [Serializable] private class TtsResponse { public string audioContent; }

    [Serializable]
    public class TtsInput
    {
        public string text;
    }

    [Serializable]
    public class TtsVoice
    {
        public string languageCode;
        public string name;
    }

    [Serializable]
    public class TtsAudioConfig
    {
        public string audioEncoding;
    }

    [Serializable]
    public class TtsRequest
    {
        public TtsInput input;
        public TtsVoice voice;
        public TtsAudioConfig audioConfig;
    }

}

