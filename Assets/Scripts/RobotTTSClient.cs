using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
using System.IO;

namespace RG.LabBot
{
    public static class RobotTTSClient
    {
        [Serializable]
        class TtsReq
        {
            public Input input;
            public Voice voice;
            public AudioConfig audioConfig;

            [Serializable] public class Input { public string text; }
            [Serializable] public class Voice { public string languageCode; public string name; }
            [Serializable] public class AudioConfig { public string audioEncoding; public double speakingRate; public double pitch; }
        }

        [Serializable] class TtsResp { public string audioContent; }

        public static async Task<string> SynthesizeToMp3FileAsync(string text, string voiceName, string languageCode = "en-US")
        {
            var token = await GoogleAuth.GetAccessTokenAsync();
            var url = "https://texttospeech.googleapis.com/v1/text:synthesize";

            var payload = new TtsReq
            {
                input = new TtsReq.Input { text = text },
                voice = new TtsReq.Voice { languageCode = languageCode, name = voiceName },
                audioConfig = new TtsReq.AudioConfig { audioEncoding = "MP3", speakingRate = 1.0, pitch = 0.0 }
            };

            var json = JsonUtility.ToJson(payload);

          //  Debug.Log("[TTS] POST synthesize");
            using var www = UnityWebRequest.Put(url, json);
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Authorization", $"Bearer {token}");
            www.SetRequestHeader("Content-Type", "application/json");

            var op = www.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
                throw new Exception($"[TTS] HTTP {(int)www.responseCode} {www.error}\nBody: {www.downloadHandler.text}");

            var resp = JsonUtility.FromJson<TtsResp>(www.downloadHandler.text);
            var bytes = Convert.FromBase64String(resp.audioContent);
           // Debug.Log($"[TTS] Received {bytes.Length} bytes of MP3 audio.");

            var path = Path.Combine(Application.persistentDataPath, $"tts_{DateTime.UtcNow.Ticks}.mp3");
            File.WriteAllBytes(path, bytes);
           // Debug.Log($"[TTS] Wrote MP3: {path} ({new FileInfo(path).Length} bytes)");

            return path;
        }
    }
}
