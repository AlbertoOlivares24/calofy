using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class ElevenLabsAudio : MonoBehaviour
{
    public enum APIVersion { V1, V3 }

    // Available voices organized by character
    public enum VoicePreset
    {
        // Recommended Voices
        River_CalmnNeutral,
        Chris_NaturalDownToEarth,
        Eric_SmoothClassy,

        // Male Voices
        Clyde_CharacterUseCase,
        Roger_EasyGoingCasual,
        Charlie_ConfidentAustralian,
        George_WarmMature,
        Callum_GravellyUnsettling,
        Harry_AnimatedWarrior,
        Liam_YoungConfident,
        Will_ConversationalLaidBack,
        Brian_ResonantComforting,
        Daniel_StrongProfessional,
        Bill_FriendlyComforting,

        // Female Voices
        Sarah_ProfessionalConfident,
        Laura_SassyQuirky,
        Jessica_YoungPlayful,
        Alice_ProfessionalBritish,
        Matilda_ProfessionalUpbeat,
        Lily_VelvetyBritish
    }

    // Voice ID mapping
    private static readonly System.Collections.Generic.Dictionary<VoicePreset, string> VoiceIdMap =
        new System.Collections.Generic.Dictionary<VoicePreset, string>()
    {
        // Recommended
        { VoicePreset.River_CalmnNeutral, "SAz9YHcvj6GT2YYXdXww" },
        { VoicePreset.Chris_NaturalDownToEarth, "iP95p4xoKVk53GoZ742B" },
        { VoicePreset.Eric_SmoothClassy, "cjVigY5qzO86Huf0OWal" },

        // Male Voices
        { VoicePreset.Clyde_CharacterUseCase, "2EiwWnXFnvU5JabPnv8n" },
        { VoicePreset.Roger_EasyGoingCasual, "CwhRBWXzGAHq8TQ4Fs17" },
        { VoicePreset.Charlie_ConfidentAustralian, "IKne3meq5aSn9XLyUdCD" },
        { VoicePreset.George_WarmMature, "JBFqnCBsd6RMkjVDRZzb" },
        { VoicePreset.Callum_GravellyUnsettling, "N2lVS1w4EtoT3dr4eOWO" },
        { VoicePreset.Harry_AnimatedWarrior, "SOYHLrjzK2X1ezoPC6cr" },
        { VoicePreset.Liam_YoungConfident, "TX3LPaxmHKxFdv7VOQHJ" },
        { VoicePreset.Will_ConversationalLaidBack, "bIHbv24MWmeRgasZH58o" },
        { VoicePreset.Brian_ResonantComforting, "nPczCjzI2devNBz1zQrb" },
        { VoicePreset.Daniel_StrongProfessional, "onwK4e9ZLuTAKqWW03F9" },
        { VoicePreset.Bill_FriendlyComforting, "pqHfZKP75CvOlQylNhV4" },

        // Female Voices
        { VoicePreset.Sarah_ProfessionalConfident, "EXAVITQu4vr4xnSDxMaL" },
        { VoicePreset.Laura_SassyQuirky, "FGY2WhTYpPnrIDTdsKH5" },
        { VoicePreset.Jessica_YoungPlayful, "cgSgspJ2msm6clMCkdW9" },
        { VoicePreset.Alice_ProfessionalBritish, "Xb7hH8MSUJpSbSDYk0k2" },
        { VoicePreset.Matilda_ProfessionalUpbeat, "XrExE9yKIg1WjnnlVkGX" },
        { VoicePreset.Lily_VelvetyBritish, "pFZP5JQG7iQjIQuC4Bku" }
    };

    [SerializeField] private string apiKey = ""; // Set in Inspector
    [SerializeField] private APIVersion apiVersion = APIVersion.V1; // Default to v1
    [SerializeField] private VoicePreset voicePreset = VoicePreset.River_CalmnNeutral; // Select from dropdown
    [SerializeField] private string modelId = "eleven_multilingual_v2"; // v3 API model
    [SerializeField] private float voiceStability = 0.5f;
    [SerializeField] private float voiceSimilarity = 0.75f;

    private string voiceId; // Will be set based on preset

    private HttpClient httpClient;
    private AudioSource audioSource;

    public delegate void OnAudioGenerated(AudioClip clip);
    public delegate void OnAudioError(string errorMessage);

    public event OnAudioGenerated AudioGenerated;
    public event OnAudioError ErrorOccurred;

    private void Awake()
    {
        httpClient = new HttpClient();

        // Get voice ID from preset
        if (VoiceIdMap.TryGetValue(voicePreset, out string id))
        {
            voiceId = id;
            Debug.Log($"[ElevenLabs] Using voice: {voicePreset} (ID: {voiceId})");
        }
        else
        {
            Debug.LogWarning($"[ElevenLabs] Unknown voice preset: {voicePreset}");
            voiceId = "SAz9YHcvj6GT2YYXdXww"; // Default to River
        }

        // Get or create AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnDestroy()
    {
        httpClient?.Dispose();
    }

    /// <summary>
    /// Convert text to speech and play it
    ///
    /// V1 API (default): Simple, stable, reliable
    /// V3 API: Supports audio tags for advanced control
    ///   - <break time="500ms" /> - Add pauses
    ///   - <emphasis level="strong">word</emphasis> - Emphasize words
    ///   - <prosody pitch="20%" rate="10%">text</prosody> - Control pitch and rate
    ///
    /// Example with tags: "Welcome! <break time=\"500ms\" /> Ready to begin?"
    /// </summary>
    public void SpeakText(string text)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            ErrorOccurred?.Invoke("ElevenLabs API key is not set. Please set it in the Inspector.");
            Debug.LogError("ElevenLabs API key not set!");
            return;
        }

        StartCoroutine(SpeakTextAsync(text));
    }

    private System.Collections.IEnumerator SpeakTextAsync(string text)
    {
        yield return StartCoroutine(GenerateAndPlayAudioCoroutine(text));
    }

    private IEnumerator GenerateAndPlayAudioCoroutine(string text)
    {
        Debug.Log($"[ElevenLabs] Converting text to speech ({apiVersion} API): {text}");

        // Prepare payload based on API version
        object payload;
        string url;

        if (apiVersion == APIVersion.V1)
        {
            // V1 API payload (simpler, no model_id)
            payload = new
            {
                text = text,
                voice_settings = new
                {
                    stability = voiceStability,
                    similarity_boost = voiceSimilarity
                }
            };
            url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}";
        }
        else // V3
        {
            // V3 API payload (includes model_id, supports audio tags)
            payload = new
            {
                text = text,
                model_id = modelId,
                voice_settings = new
                {
                    stability = voiceStability,
                    similarity_boost = voiceSimilarity
                }
            };
            url = $"https://api.elevenlabs.io/v3/text-to-speech/{voiceId}";
        }

        // Make request
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("xi-api-key", apiKey);

        var content = new StringContent(
            Newtonsoft.Json.JsonConvert.SerializeObject(payload),
            Encoding.UTF8,
            "application/json"
        );

        var task = httpClient.PostAsync(url, content);
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsFaulted)
        {
            ErrorOccurred?.Invoke($"HTTP Error: {task.Exception?.InnerException?.Message}");
            Debug.LogError($"[ElevenLabs] Request failed: {task.Exception}");
            yield break;
        }

        var response = task.Result;

        if (!response.IsSuccessStatusCode)
        {
            var errorTaskContent = response.Content.ReadAsStringAsync();
            while (!errorTaskContent.IsCompleted)
            {
                yield return null;
            }
            string errorBody = errorTaskContent.Result;
            ErrorOccurred?.Invoke($"HTTP {(int)response.StatusCode}: {errorBody}");
            Debug.LogError($"[ElevenLabs] API Error: {errorBody}");
            yield break;
        }

        // Get audio bytes
        var audioTaskContent = response.Content.ReadAsByteArrayAsync();
        while (!audioTaskContent.IsCompleted)
        {
            yield return null;
        }
        byte[] audioBytes = audioTaskContent.Result;
        Debug.Log($"[ElevenLabs] Received audio ({audioBytes.Length} bytes)");

        // Convert bytes to AudioClip using UnityWebRequest
        yield return StartCoroutine(LoadAudioClipFromBytes(audioBytes));
    }

    private IEnumerator LoadAudioClipFromBytes(byte[] mp3Bytes)
    {
        // Save MP3 bytes to temporary file
        string tempPath = System.IO.Path.Combine(Application.temporaryCachePath, "elevenlabs_temp.mp3");
        System.IO.File.WriteAllBytes(tempPath, mp3Bytes);

        // Load using UnityWebRequest (handles MP3 decoding)
        string fileUrl = "file:///" + tempPath;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                audioClip.name = "ElevenLabsAudio";
                Debug.Log($"[ElevenLabs] AudioClip created: {audioClip.length} seconds");
                Debug.Log($"[ElevenLabs] Playing audio");
                audioSource.PlayOneShot(audioClip);
                AudioGenerated?.Invoke(audioClip);
            }
            else
            {
                ErrorOccurred?.Invoke($"Failed to load audio: {www.error}");
                Debug.LogError($"[ElevenLabs] Failed to load audio: {www.error}");
            }
        }
    }

}
