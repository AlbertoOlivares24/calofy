using UnityEngine;

namespace MRTemplate.Audio
{
    /// <summary>
    /// ScriptableObject configuration for ElevenLabs TTS integration.
    /// Create an instance via: Assets > Create > MRTemplate > ElevenLabs Config
    /// </summary>
    [CreateAssetMenu(menuName = "MRTemplate/ElevenLabs Config", fileName = "ElevenLabsConfig", order = 300)]
    public class ElevenLabsConfig : ScriptableObject
    {
        [SerializeField]
        [Tooltip("API key from ElevenLabs dashboard (https://elevenlabs.io/app/api-keys)")]
        private string _apiKey = "";

        [SerializeField]
        [Tooltip("Default voice ID to use for TTS")]
        private string _defaultVoiceId = "";

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Voice stability (0 = variety, 1 = consistency)")]
        private float _voiceStability = 0.5f;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Similarity boost (0 = less similarity, 1 = more similarity)")]
        private float _similarityBoost = 0.75f;

        [SerializeField]
        [Tooltip("Enable caching of TTS responses")]
        private bool _enableCaching = true;

        [SerializeField]
        [Tooltip("Cache directory name (relative to persistentDataPath)")]
        private string _cacheDirectory = "elevenlabs_tts_cache";

        public string ApiKey => _apiKey;
        public string DefaultVoiceId => _defaultVoiceId;
        public float VoiceStability => _voiceStability;
        public float SimilarityBoost => _similarityBoost;
        public bool EnableCaching => _enableCaching;
        public string CacheDirectory => _cacheDirectory;

        /// <summary>
        /// Validates configuration before use
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                Debug.LogError("[ElevenLabsConfig] API key is not set");
                return false;
            }

            if (string.IsNullOrEmpty(_defaultVoiceId))
            {
                Debug.LogWarning("[ElevenLabsConfig] Default voice ID is not set");
                return false;
            }

            return true;
        }
    }
}
