using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RG.LabBot
{
    public static class RobotGeminiClient
    {
        [Serializable]
        class Req
        {
            public List<Content> contents;
            [Serializable] public class Content { public string role; public List<Part> parts; }
            [Serializable] public class Part { public string text; }
        }

        [Serializable]
        class Resp
        {
            public List<Candidate> candidates;
            [Serializable] public class Candidate { public Content content; }
            [Serializable] public class Content { public List<Part> parts; }
            [Serializable] public class Part { public string text; }
        }

        public static async Task<string> GenerateAsync(string projectId, string region, string model, string prompt)
        {
            var token = await GoogleAuth.GetAccessTokenAsync();
            var url = $"https://{region}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{region}/publishers/google/models/{model}:generateContent";

            var req = new Req
            {
                contents = new List<Req.Content> {
                    new Req.Content {
                        role = "user",
                        parts = new List<Req.Part> { new Req.Part { text = prompt } }
                    }
                }
            };
            var json = JsonUtility.ToJson(req);

           // Debug.Log($"[Gemini] POST {url}");
            using var www = UnityWebRequest.Put(url, json);
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Authorization", $"Bearer {token}");
            www.SetRequestHeader("Content-Type", "application/json");

            var op = www.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Gemini] HTTP {(int)www.responseCode} {www.error}\nBody: {www.downloadHandler.text}");
                return null;
            }

            var body = www.downloadHandler.text;
            // Uncomment to see raw: Debug.Log($"[Gemini] Raw: {body}");
            var resp = JsonUtility.FromJson<Resp>(body);
            var text = resp?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text;
            return text?.Trim();



        }
    }
}
