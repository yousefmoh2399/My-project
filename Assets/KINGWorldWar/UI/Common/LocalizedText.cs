using KingWorldWar.Core.Localization;
using TMPro;
using UnityEngine;

namespace KingWorldWar.UI.Common
{
    /// <summary>
    /// Synchronizes a TextMeshProUGUI element with the active localization language and text shaping.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        [Tooltip("Stable localization key defined in string tables.")]
        [SerializeField] private string _key;

        [Tooltip("Whether to mirror text alignment to Right when language is RTL (Arabic).")]
        [SerializeField] private bool _autoAlignRTL = true;

        private TextMeshProUGUI _tmp;
        private ILocalizationService _localizationService;
        private object[] _formatArgs;

        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                RefreshText();
            }
        }

        private void Awake()
        {
            _tmp = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            RefreshText();
        }

        public void Bind(ILocalizationService localizationService)
        {
            if (_localizationService != null)
            {
                _localizationService.OnLanguageChanged -= OnLanguageChanged;
            }

            _localizationService = localizationService;

            if (_localizationService != null)
            {
                _localizationService.OnLanguageChanged += OnLanguageChanged;
            }

            RefreshText();
        }

        private void OnDestroy()
        {
            if (_localizationService != null)
            {
                _localizationService.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        public void SetArgs(params object[] args)
        {
            _formatArgs = args;
            RefreshText();
        }

        private void OnLanguageChanged(Language newLang)
        {
            RefreshText();
        }

        public void RefreshText()
        {
            if (_tmp == null)
            {
                _tmp = GetComponent<TextMeshProUGUI>();
                if (_tmp == null) return;
            }

            if (string.IsNullOrEmpty(_key)) return;

            if (_localizationService != null)
            {
                _tmp.text = _localizationService.GetString(_key, _formatArgs);

                if (_autoAlignRTL)
                {
                    _tmp.alignment = _localizationService.IsRightToLeft
                        ? TextAlignmentOptions.MidlineRight
                        : TextAlignmentOptions.MidlineLeft;
                }
            }
            else
            {
                _tmp.text = $"[key:{_key}]";
            }
        }
    }
}
