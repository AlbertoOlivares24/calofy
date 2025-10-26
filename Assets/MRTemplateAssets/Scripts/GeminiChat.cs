using UnityEngine;
using System.Collections;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json.Linq;

public class GeminiChat : MonoBehaviour
{
    [SerializeField] private string projectId = "starry-argon-473718-t5";
    [SerializeField] private string location = "us-central1";
    [SerializeField] private string modelId = "gemini-2.0-flash-001";
    [SerializeField] private string systemPrompt = "You are a witty, upbeat game-show host. Keep replies under 6 seconds. Keep it PG and appropriate. No special characters, your response should just be words, letters, periods or commas. Nothing else.";

    private HttpClient httpClient;
    private string cachedAccessToken;
    private System.DateTime tokenExpiry;

    public delegate void OnGeminiResponseReceived(string response);
    public delegate void OnGeminiError(string errorMessage);

    public event OnGeminiResponseReceived ResponseReceived;
    public event OnGeminiError ErrorOccurred;

    private void Awake()
    {
        httpClient = new HttpClient();
    }

    private void OnDestroy()
    {
        httpClient?.Dispose();
    }

    /// <summary>
    /// Request a response from Gemini API
    /// </summary>
    public void SendPrompt(string userPrompt)
    {
        StartCoroutine(SendPromptAsync(userPrompt));
    }

    private IEnumerator SendPromptAsync(string userPrompt)
    {
        var task = GetGeminiResponse(userPrompt);
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsFaulted)
        {
            ErrorOccurred?.Invoke($"Error: {task.Exception?.InnerException?.Message}");
        }
    }

    private async Task GetGeminiResponse(string userPrompt)
    {
        try
        {
            // Get access token
            string token = await GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                ErrorOccurred?.Invoke("Failed to obtain access token");
                return;
            }

            // Prepare payload
            var payload = new
            {
                systemInstruction = new
                {
                    role = "user",
                    parts = new[] { new { text = systemPrompt } }
                },
                contents = new[] {
                    new {
                        role = "user",
                        parts = new[] { new { text = userPrompt } }
                    }
                }
            };

            // Build URL
            string url = $"https://{location}-aiplatform.googleapis.com/v1/projects/{projectId}/locations/{location}/publishers/google/models/{modelId}:generateContent";

            // Make request
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var content = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                ErrorOccurred?.Invoke($"HTTP {(int)response.StatusCode}: {errorBody}");
                return;
            }

            // Parse response
            string responseBody = await response.Content.ReadAsStringAsync();
            JObject jsonResponse = JObject.Parse(responseBody);

            string geminiResponse = jsonResponse["candidates"][0]["content"]["parts"][0]["text"]?.ToString() ?? "(no text)";

            ResponseReceived?.Invoke(geminiResponse);
        }
        catch (System.Exception ex)
        {
            ErrorOccurred?.Invoke($"Exception: {ex.Message}");
            Debug.LogError($"GeminiChat Error: {ex}");
        }
    }

    private async Task<string> GetAccessToken()
    {
        try
        {
            // Return cached token if still valid
            if (!string.IsNullOrEmpty(cachedAccessToken) && System.DateTime.UtcNow < tokenExpiry)
            {
                return cachedAccessToken;
            }

            // Get application default credentials
            var credential = await GoogleCredential.GetApplicationDefaultAsync();
            credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");

            // Get new token
            var token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            cachedAccessToken = token;
            tokenExpiry = System.DateTime.UtcNow.AddMinutes(59); // Tokens typically last 1 hour

            return token;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to get access token: {ex.Message}");
            return null;
        }
    }
}