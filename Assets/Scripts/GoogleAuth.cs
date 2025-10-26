using Google.Apis.Auth.OAuth2;
using System.Threading.Tasks;
using System;

public static class GoogleAuth
{
    static ServiceAccountCredential cred;
    static readonly string[] scopes = { "https://www.googleapis.com/auth/cloud-platform" };

    public static void ConfigureServiceAccount(string jsonPath)
    {
        cred = GoogleCredential
            .FromFile(jsonPath)
            .CreateScoped(scopes)
            .UnderlyingCredential as ServiceAccountCredential;

        if (cred == null) throw new Exception("Failed to load service account.");
    }

    public static async Task<string> GetAccessTokenAsync()
    {
        if (cred == null) throw new Exception("Call ConfigureServiceAccount(jsonPath) first.");
        await cred.RequestAccessTokenAsync(System.Threading.CancellationToken.None);
        return cred.Token.AccessToken;
    }
}
