using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Google.Apis.Auth.OAuth2;

public class SttDemo : MonoBehaviour
{
    [Header("UI (optional)")]
    public UnityEngine.UI.Text transcriptText; // leave null if you just want Console logs

    [Header("Mic Settings")]
    public string microphoneDevice = null; // default device
    public int sampleRate = 16000;         // recommended for speech
    public int maxSeconds = 59;            // sync STT supports ≤60s

    [Header("STT Config")]
    public string languageCode = "en-US";
    public bool enableAutomaticPunctuation = true;

    private AudioClip recording;
    private string wavPath => Path.Combine(Application.persistentDataPath, "stt.wav");

    // ---------- Public methods to wire to buttons ----------
    bool isRecording = false;
    string activeDevice = null;

    public async void StartRecording()
    {
        // Ask for permission
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            await Application.RequestUserAuthorization(UserAuthorization.Microphone);
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                Debug.LogError("Microphone permission denied.");
                return;
            }
        }

        // Pick first available mic
        if (Microphone.devices == null || Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone devices found.");
            return;
        }
        activeDevice = Microphone.devices[0];
        Debug.Log($"Using mic: {activeDevice}");

        // Start recording
        recording = Microphone.Start(activeDevice, false, maxSeconds, sampleRate);
        if (recording == null)
        {
            Debug.LogError("Microphone.Start returned null. Try a different sampleRate (44100) or check system settings.");
            return;
        }

        // Wait until mic buffer actually starts
        while (Microphone.GetPosition(activeDevice) <= 0)
            await Task.Yield();

        isRecording = true;
        Debug.Log("🎙️ Recording started!");
    }

    public async void StopAndTranscribe()
    {
        if (!isRecording || recording == null)
        {
            Debug.LogWarning("Not recording — did you press Start first?");
            return;
        }

        Microphone.End(activeDevice);
        isRecording = false;
        Debug.Log("🎧 Recording stopped, encoding...");

        byte[] wav = EncodeWavMonoPcm16(recording, sampleRate);
        string path = Path.Combine(Application.persistentDataPath, "stt.wav");
        File.WriteAllBytes(path, wav);
        Debug.Log("Saved WAV: " + path);

        string transcript = await RecognizeAsync(wav, sampleRate, languageCode, enableAutomaticPunctuation);
        Debug.Log("Transcript: " + transcript);
    }

    // ---------- Google STT call ----------
    private async Task<string> RecognizeAsync(byte[] wavLinear16, int sr, string lang, bool punct)
    {
        // Load YOUR STT key: Assets/Resources/service_stt.json
        var jsonKey = Resources.Load<TextAsset>("service_stt");
        if (jsonKey == null) { Debug.LogError("Missing Resources/service_stt.json"); return "(missing key)"; }

        var cred = GoogleCredential.FromJson(jsonKey.text)
            .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
        string token = await cred.UnderlyingCredential.GetAccessTokenForRequestAsync();

        var req = new SttRecognizeRequest
        {
            config = new RecognitionConfig
            {
                encoding = "LINEAR16",
                sampleRateHertz = sr,
                languageCode = lang,
                enableAutomaticPunctuation = punct
            },
            audio = new RecognitionAudio { content = Convert.ToBase64String(wavLinear16) }
        };
        string jsonBody = JsonUtility.ToJson(req);

        using (var www = new UnityWebRequest("https://speech.googleapis.com/v1/speech:recognize", "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
            www.SetRequestHeader("Authorization", $"Bearer {token}");

            var op = www.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"STT failed: {www.responseCode} {www.error}\n{www.downloadHandler.text}");
                return "(error)";
            }

            var resp = JsonUtility.FromJson<SttRecognizeResponse>(www.downloadHandler.text);
            return ExtractTranscript(resp);
        }
    }

    // ---------- JSON models ----------
    [Serializable]
    class RecognitionConfig
    {
        public string encoding; public int sampleRateHertz; public string languageCode; public bool enableAutomaticPunctuation;
    }
    [Serializable] class RecognitionAudio { public string content; }
    [Serializable] class SttRecognizeRequest { public RecognitionConfig config; public RecognitionAudio audio; }

    [Serializable] class SttAlternative { public string transcript; public float confidence; }
    [Serializable] class SttResult { public SttAlternative[] alternatives; }
    [Serializable] class SttRecognizeResponse { public SttResult[] results; }

    private string ExtractTranscript(SttRecognizeResponse r)
    {
        if (r?.results == null || r.results.Length == 0) return "(no speech detected)";
        var alt = r.results[0].alternatives;
        return (alt != null && alt.Length > 0) ? alt[0].transcript : "(empty)";
    }

    // ---------- WAV encoder (mono, PCM16) ----------
    private byte[] EncodeWavMonoPcm16(AudioClip clip, int hz)
    {
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        // mixdown to mono
        int n = clip.samples;
        float[] mono = new float[n];
        for (int i = 0; i < n; i++)
        {
            float sum = 0f;
            for (int c = 0; c < clip.channels; c++) sum += samples[i * clip.channels + c];
            mono[i] = sum / clip.channels;
        }

        // float [-1,1] -> PCM16
        short[] pcm = new short[mono.Length];
        for (int i = 0; i < mono.Length; i++)
        {
            float v = Mathf.Clamp(mono[i], -1f, 1f);
            pcm[i] = (short)Mathf.RoundToInt(v * short.MaxValue);
        }

        using (var ms = new MemoryStream())
        using (var bw = new BinaryWriter(ms))
        {
            int byteRate = hz * 2; // mono 16-bit
            int sub2 = pcm.Length * 2;
            int chunk = 36 + sub2;

            // RIFF/WAVE header
            bw.Write(Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(chunk);
            bw.Write(Encoding.ASCII.GetBytes("WAVE"));
            bw.Write(Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16);                 // PCM
            bw.Write((short)1);           // format
            bw.Write((short)1);           // channels
            bw.Write(hz);                 // sample rate
            bw.Write(byteRate);           // byte rate
            bw.Write((short)2);           // block align
            bw.Write((short)16);          // bits per sample

            bw.Write(Encoding.ASCII.GetBytes("data"));
            bw.Write(sub2);
            foreach (var s in pcm) bw.Write(s);

            return ms.ToArray();
        }
    }
}
