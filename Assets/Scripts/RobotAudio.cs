using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace RG.LabBot
{
    public static class RobotAudio
    {
        public static async Task PlayMp3Async(AudioSource speaker, string filePath)
        {
            if (speaker == null) { Debug.LogError("[Audio] speaker is null"); return; }
            var url = "file://" + filePath;
           // Debug.Log($"[Audio] Loading: {url}");

            using var www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
            var op = www.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Audio] Load failed: {www.error}");
                return;
            }

            var clip = DownloadHandlerAudioClip.GetContent(www);
            if (clip == null) { Debug.LogError("[Audio] Clip is null"); return; }

          //  Debug.Log($"[Audio] Clip length: {clip.length:0.00}s, channels: {clip.channels}, freq: {clip.frequency}");
          //  Debug.Log($"[Audio] Speaker settings — vol:{speaker.volume}, mute:{speaker.mute}");

            speaker.clip = clip;
            speaker.Play();
          //  Debug.Log("[Audio] Playing.");
        }
    }
}
