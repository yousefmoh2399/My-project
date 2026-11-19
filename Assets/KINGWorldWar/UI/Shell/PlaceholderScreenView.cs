using KingWorldWar.Core.Localization;
using KingWorldWar.Core.UI;
using TMPro;
using UnityEngine;

namespace KingWorldWar.UI.Shell
{
    /// <summary>
    /// Generic view implementation for shell placeholder gameplay screens.
    /// </summary>
    public class PlaceholderScreenView : MonoBehaviour, IScreen
    {
        [SerializeField] private ScreenId _screenId;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descText;
        [SerializeField] private TextMeshProUGUI _phaseBadgeText;

        public ScreenId ScreenId => _screenId;
        public bool IsVisible => gameObject.activeSelf;

        public void Initialize(ScreenId screenId, string titleKey, string descKey, string phaseBadge, ILocalizationService loc)
        {
            _screenId = screenId;

            if (_titleText != null)
            {
                _titleText.text = loc != null ? loc.GetString(titleKey) : titleKey;
            }

            if (_descText != null)
            {
                _descText.text = loc != null ? loc.GetString(descKey) : descKey;
            }

            if (_phaseBadgeText != null)
            {
                _phaseBadgeText.text = phaseBadge;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
