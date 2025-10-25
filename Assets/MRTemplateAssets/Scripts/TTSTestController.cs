using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MRTemplate.Audio
{
    /// <summary>
    /// Simple test controller for ElevenLabs TTS functionality.
    /// Provides UI for text input, voice selection, and playback control.
    /// </summary>
    public class TTSTestController : MonoBehaviour
    {
        [SerializeField]
        private ElevenLabsTTSManager _ttsManager;

        [SerializeField]
        private TMP_InputField _textInput;

        [SerializeField]
        private Button _speakButton;

        [SerializeField]
        private Button _stopButton;

        [SerializeField]
        private TMP_Dropdown _voiceDropdown;

        [SerializeField]
        private TextMeshProUGUI _statusText;

        [SerializeField]
        private Slider _stabilitySlider;

        [SerializeField]
        private Slider _similaritySlider;

        [SerializeField]
        private TextMeshProUGUI _stabilityLabel;

        [SerializeField]
        private TextMeshProUGUI _similarityLabel;

        private ElevenLabsClient.Voice[] _availableVoices;
        private string _selectedVoiceId;

        private void OnEnable()
        {
            if (_ttsManager == null)
            {
                Debug.LogError("[TTSTestController] TTSManager is not assigned");
                enabled = false;
                return;
            }

            _ttsManager.OnPlaybackStarted += OnPlaybackStarted;
            _ttsManager.OnPlaybackEnded += OnPlaybackEnded;
            _ttsManager.OnError += OnPlaybackError;

            SetupUI();
            UpdateStatus("Ready");
        }

        private void OnDisable()
        {
            if (_ttsManager == null) return;

            _ttsManager.OnPlaybackStarted -= OnPlaybackStarted;
            _ttsManager.OnPlaybackEnded -= OnPlaybackEnded;
            _ttsManager.OnError -= OnPlaybackError;
        }

        private void SetupUI()
        {
            // Setup buttons
            if (_speakButton != null)
                _speakButton.onClick.AddListener(OnSpeakButtonClicked);

            if (_stopButton != null)
                _stopButton.onClick.AddListener(OnStopButtonClicked);

            // Setup voice dropdown
            if (_voiceDropdown != null)
            {
                _availableVoices = _ttsManager.GetAvailableVoices();

                if (_availableVoices.Length == 0)
                {
                    UpdateStatus("No voices available. Check API key.");
                    return;
                }

                _voiceDropdown.ClearOptions();
                var options = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();

                foreach (var voice in _availableVoices)
                {
                    options.Add(new TMP_Dropdown.OptionData(voice.Name));
                }

                _voiceDropdown.AddOptions(options);
                _voiceDropdown.onValueChanged.AddListener(OnVoiceSelected);

                if (_availableVoices.Length > 0)
                {
                    _selectedVoiceId = _availableVoices[0].VoiceId;
                }
            }

            // Setup sliders
            if (_stabilitySlider != null)
            {
                _stabilitySlider.value = 0.5f;
                _stabilitySlider.onValueChanged.AddListener(OnStabilityChanged);
            }

            if (_similaritySlider != null)
            {
                _similaritySlider.value = 0.75f;
                _similaritySlider.onValueChanged.AddListener(OnSimilarityChanged);
            }

            // Set default text
            if (_textInput != null && string.IsNullOrEmpty(_textInput.text))
            {
                _textInput.text = "Hello! This is a test of text-to-speech functionality.";
            }
        }

        private void OnSpeakButtonClicked()
        {
            if (_textInput == null || string.IsNullOrEmpty(_textInput.text))
            {
                UpdateStatus("Please enter text to speak");
                return;
            }

            if (string.IsNullOrEmpty(_selectedVoiceId))
            {
                UpdateStatus("Please select a voice");
                return;
            }

            UpdateStatus("Generating speech...");
            _speakButton.interactable = false;

            var voiceSettings = new ElevenLabsClient.VoiceSettings(
                stability: _stabilitySlider != null ? _stabilitySlider.value : 0.5f,
                similarityBoost: _similaritySlider != null ? _similaritySlider.value : 0.75f
            );

            _ttsManager.QueueTTSRequest(
                text: _textInput.text,
                voiceId: _selectedVoiceId,
                onSuccess: (clip) =>
                {
                    UpdateStatus($"Playing: {_textInput.text.Substring(0, System.Math.Min(30, _textInput.text.Length))}...");
                },
                onError: (error) =>
                {
                    UpdateStatus($"Error: {error}");
                    _speakButton.interactable = true;
                },
                voiceSettings: voiceSettings
            );
        }

        private void OnStopButtonClicked()
        {
            _ttsManager.StopAllAudio();
            _speakButton.interactable = true;
            UpdateStatus("Stopped");
        }

        private void OnVoiceSelected(int index)
        {
            if (_availableVoices != null && index >= 0 && index < _availableVoices.Length)
            {
                _selectedVoiceId = _availableVoices[index].VoiceId;
                Debug.Log($"[TTSTestController] Selected voice: {_availableVoices[index].Name} ({_selectedVoiceId})");
            }
        }

        private void OnStabilityChanged(float value)
        {
            if (_stabilityLabel != null)
            {
                _stabilityLabel.text = $"Stability: {value:F2}";
            }
        }

        private void OnSimilarityChanged(float value)
        {
            if (_similarityLabel != null)
            {
                _similarityLabel.text = $"Similarity: {value:F2}";
            }
        }

        private void OnPlaybackStarted()
        {
            UpdateStatus("Playing...");
        }

        private void OnPlaybackEnded()
        {
            _speakButton.interactable = true;
            UpdateStatus("Ready");
        }

        private void OnPlaybackError(string error)
        {
            UpdateStatus($"Error: {error}");
            _speakButton.interactable = true;
        }

        private void UpdateStatus(string message)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
                Debug.Log($"[TTSTestController] Status: {message}");
            }
        }
    }
}
