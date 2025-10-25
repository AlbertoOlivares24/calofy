using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using MRTemplate.Audio;

  public class TTSAudioTest : MonoBehaviour
  {
      [SerializeField] ElevenLabsTTSManager ttsManager;
      [SerializeField] TMP_InputField textInput;
      [SerializeField] Button speakButton;
      [SerializeField] TextMeshProUGUI statusText;

      void Start()
      {
          // Hook up button click
          speakButton.onClick.AddListener(OnSpeakClicked);

          Debug.Log("TTS Test Ready. Click 'Speak' button to test audio.");
          UpdateStatus("Ready");
      }

      void OnSpeakClicked()
      {
          string textToSpeak = textInput.text;

          if (string.IsNullOrEmpty(textToSpeak))
          {
              UpdateStatus("Enter text first!");
              return;
          }

          UpdateStatus("Generating audio...");
          Debug.Log($"Requesting TTS for: '{textToSpeak}'");

          // Queue the TTS request
          ttsManager.QueueTTSRequest(
              text: textToSpeak,
              voiceId: null,  // Uses default voice from config
              onSuccess: (audioClip) =>
              {
                  UpdateStatus($"Playing ({audioClip.length:F1}s)");
                  Debug.Log($"SUCCESS! Audio: {audioClip.name}, Length: {audioClip.length:F2}s");

                  // Audio auto-plays via AudioSource
              },
              onError: (error) =>
              {
                  UpdateStatus($"Error: {error}");
                  Debug.LogError($"TTS Error: {error}");
              }
          );
      }

      void Update()
      {
          // Also allow Space key for testing (using new Input System)
          if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
          {
              OnSpeakClicked();
          }
      }

      void UpdateStatus(string message)
      {
          statusText.text = message;
          Debug.Log($"[Status] {message}");
      }
  }
