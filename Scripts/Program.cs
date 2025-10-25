using Google.Apis.Auth.OAuth2;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

const string PROJECT_ID = "starry-argon-473718-t5";
const string LOCATION = "us-central1";
const string MODEL_ID = "gemini-2.0-flash-001";

var cred = await GoogleCredential.GetApplicationDefaultAsync();
cred = cred.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
var token = await cred.UnderlyingCredential.GetAccessTokenForRequestAsync();

var payload = new
{
    systemInstruction = new
    {
        role = "system",
        parts = new[] { new { text = "You are a witty, upbeat game-show host. Keep replies under 6 seconds. Keep it PG and appropriate. No special characters, your response should just be words, letters, periods or commas. Nothing else." } }
    },
    contents = new[] {
    new { role = "user", parts = new[] { new { text = "Welcome everyone to round one and introduce yourself as the host of this experience!" } } }
  }
};

var url = $"https://{LOCATION}-aiplatform.googleapis.com/v1/projects/{PROJECT_ID}/locations/{LOCATION}/publishers/google/models/{MODEL_ID}:generateContent";
using var http = new HttpClient();
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

var httpResp = await http.PostAsync(
  url,
  new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
);

var body = await httpResp.Content.ReadAsStringAsync();
if (!httpResp.IsSuccessStatusCode)
{
    Console.WriteLine($"HTTP {(int)httpResp.StatusCode} {httpResp.StatusCode}");
    Console.WriteLine(body);
    return;
}

using var doc = JsonDocument.Parse(body);
string hostText = doc.RootElement
  .GetProperty("candidates")[0]
  .GetProperty("content").GetProperty("parts")[0]
  .GetProperty("text").GetString() ?? "(no text)";

Console.WriteLine("\nGemini said:\n" + hostText);