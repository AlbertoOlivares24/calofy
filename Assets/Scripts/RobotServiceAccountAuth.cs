using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace RG.LabBot
{
    /// Pure-Unity service-account OAuth for Google APIs (no external packages).
    public static class RobotServiceAccountAuth
    {
        static string _cachedToken;
        static DateTime _expiresAtUtc;
        const string Scope = "https://www.googleapis.com/auth/cloud-platform";
        const string TokenUrl = "https://oauth2.googleapis.com/token";

        class SA { public string client_email; public string private_key; }
        static SA _sa;

        public static void Configure(string jsonPath)
        {
            var json = System.IO.File.ReadAllText(jsonPath);
            _sa = JsonUtility.FromJson<SA>(json);
            if (_sa == null || string.IsNullOrWhiteSpace(_sa.client_email) || string.IsNullOrWhiteSpace(_sa.private_key))
                throw new Exception("Service account JSON missing client_email/private_key.");
        }

        public static async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _expiresAtUtc)
                return _cachedToken;

            if (_sa == null) throw new Exception("Call RobotServiceAccountAuth.Configure(jsonPath) first.");

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long exp = now + 3600;

            string headerJson = "{\"alg\":\"RS256\",\"typ\":\"JWT\"}";
            string claimJson = $"{{\"iss\":\"{_sa.client_email}\",\"scope\":\"{Scope}\",\"aud\":\"{TokenUrl}\",\"exp\":{exp},\"iat\":{now}}}";

            string signingInput = B64Url(Encoding.UTF8.GetBytes(headerJson)) + "." +
                                  B64Url(Encoding.UTF8.GetBytes(claimJson));

            byte[] signature = SignRs256(signingInput, _sa.private_key);
            string jwt = signingInput + "." + B64Url(signature);

            var form = new WWWForm();
            form.AddField("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer");
            form.AddField("assertion", jwt);

            using var www = UnityWebRequest.Post(TokenUrl, form);
            var op = www.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
                throw new Exception($"Token exchange failed: {www.responseCode} {www.error}\n{www.downloadHandler.text}");

            var resp = JsonUtility.FromJson<TokenResp>(www.downloadHandler.text);
            if (resp == null || string.IsNullOrEmpty(resp.access_token))
                throw new Exception("Token parse error: " + www.downloadHandler.text);

            _cachedToken = resp.access_token;
            _expiresAtUtc = DateTime.UtcNow.AddSeconds(Mathf.Max(0, resp.expires_in - 300));
            return _cachedToken;
        }

        [Serializable] class TokenResp { public string access_token; public int expires_in; public string token_type; }

        static string B64Url(byte[] input) =>
            Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        static byte[] SignRs256(string signingInput, string pemPrivateKey)
        {
            var m = Regex.Match(pemPrivateKey, "-----BEGIN PRIVATE KEY-----(?<key>[^-]+)-----END PRIVATE KEY-----", RegexOptions.Singleline);
            if (!m.Success) throw new Exception("Expected PKCS#8 'BEGIN PRIVATE KEY' in service account JSON.");

            string b64 = m.Groups["key"].Value.Replace("\n", "").Replace("\r", "").Trim();
            byte[] pkcs8 = Convert.FromBase64String(b64);

            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(pkcs8, out _);

            byte[] data = Encoding.UTF8.GetBytes(signingInput);
            return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
}
