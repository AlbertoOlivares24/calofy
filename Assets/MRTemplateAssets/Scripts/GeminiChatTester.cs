using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GeminiChatTester : MonoBehaviour
{
    [SerializeField] private GeminiChat geminiChat;
    [SerializeField] private ElevenLabsAudio elevenLabsAudio;
    [SerializeField] private TMP_InputField promptInputField;
    [SerializeField] private TextMeshProUGUI responseDisplayText;
    [SerializeField] private string defaultPrompt = "Welcome everyone to round one and introduce yourself as the host of this experience!";
    [SerializeField] private bool autoPlayAudio = true;

    private bool isWaitingForResponse = false;

    private void Start()
    {
        Debug.Log("=== GeminiChat Test Ready ===");
        Debug.Log("Type a prompt in the input field and click Send");
        Debug.Log("Make sure you have set up Google credentials!");

        // Set default prompt in input field
        if (promptInputField != null)
        {
            promptInputField.text = defaultPrompt;
        }

        // Hook up events
        geminiChat.ResponseReceived += OnGeminiResponse;
        geminiChat.ErrorOccurred += OnGeminiError;

        // Display initial message
        if (responseDisplayText != null)
        {
            responseDisplayText.text = "Ready to send prompts to Gemini!\nClick Send button to test.";
        }
    }

    private void OnDestroy()
    {
        // Unhook events
        geminiChat.ResponseReceived -= OnGeminiResponse;
        geminiChat.ErrorOccurred -= OnGeminiError;
    }

    /// <summary>
    /// Called by Send button
    /// </summary>
    public void SendPromptToGemini()
    {
        if (isWaitingForResponse)
        {
            Debug.LogWarning("Already waiting for a response. Please wait...");
            return;
        }

        // Get prompt from input field or use default
        string prompt = promptInputField != null && !string.IsNullOrEmpty(promptInputField.text)
            ? promptInputField.text
            : defaultPrompt;

        Debug.Log($"Sending custom prompt to Gemini:\n{prompt}");

        if (responseDisplayText != null)
        {
            responseDisplayText.text = "Waiting for Gemini response...";
        }

        isWaitingForResponse = true;
        geminiChat.SendPrompt(prompt);
    }

    private void OnGeminiResponse(string response)
    {
        isWaitingForResponse = false;
        Debug.Log($"SUCCESS! Gemini Response:\n{response}");

        if (responseDisplayText != null)
        {
            responseDisplayText.text = $"Gemini Response:\n\n{response}";
        }

        // Send response to ElevenLabs for text-to-speech
        if (autoPlayAudio && elevenLabsAudio != null)
        {
            Debug.Log("[Tester] Sending response to ElevenLabs for speech synthesis...");
            elevenLabsAudio.SpeakText(response);
        }
    }

    private void OnGeminiError(string errorMessage)
    {
        isWaitingForResponse = false;
        Debug.LogError($"ERROR! Gemini failed:\n{errorMessage}");

        if (responseDisplayText != null)
        {
            responseDisplayText.text = $"Error:\n\n{errorMessage}";
        }
    }
}
