using System;
using UnityEngine;
using UnityEngine.UI;

namespace KingWorldWar.UI.Common
{
    public enum ButtonStyleType
    {
        Primary,   // Gold / military green emphasis
        Secondary, // Steel gray
        Danger     // Red alert
    }

    /// <summary>
    /// Production tactical button adhering to mobile accessibility minimum touch target sizes (>= 48x48dp)
    /// and WCAG contrast compliant palettes.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class KwwButton : MonoBehaviour
    {
        public const float MinTouchTargetWidth = 48f;
        public const float MinTouchTargetHeight = 48f;

        [SerializeField] private ButtonStyleType _style = ButtonStyleType.Primary;
        [SerializeField] private bool _enforceMinTouchTarget = true;

        private Button _button;
        private RectTransform _rectTransform;
        private Image _targetGraphic;
        private bool _isLoading;

        public Button Button => _button != null ? _button : (_button = GetComponent<Button>());
        public bool IsLoading => _isLoading;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _rectTransform = GetComponent<RectTransform>();
            _targetGraphic = _button.targetGraphic as Image;

            if (_enforceMinTouchTarget && _rectTransform != null)
            {
                EnforceMinimumTouchTarget();
            }

            ApplyStyle();
        }

        public void EnforceMinimumTouchTarget()
        {
            if (_rectTransform == null) return;
            Vector2 size = _rectTransform.sizeDelta;
            if (size.x < MinTouchTargetWidth) size.x = MinTouchTargetWidth;
            if (size.y < MinTouchTargetHeight) size.y = MinTouchTargetHeight;
            _rectTransform.sizeDelta = size;
        }

        public void SetLoading(bool loading)
        {
            _isLoading = loading;
            if (_button != null)
            {
                _button.interactable = !loading;
            }
        }

        public void ApplyStyle()
        {
            if (_button == null) return;

            var colors = _button.colors;
            colors.colorMultiplier = 1f;

            switch (_style)
            {
                case ButtonStyleType.Primary:
                    // Gold accent on dark charcoal
                    colors.normalColor = new Color(0.83f, 0.69f, 0.22f, 1.0f);     // #D4AF37
                    colors.highlightedColor = new Color(0.92f, 0.78f, 0.30f, 1.0f);
                    colors.pressedColor = new Color(0.70f, 0.58f, 0.18f, 1.0f);
                    colors.disabledColor = new Color(0.40f, 0.40f, 0.40f, 0.6f);
                    break;

                case ButtonStyleType.Secondary:
                    // Steel gray
                    colors.normalColor = new Color(0.29f, 0.33f, 0.41f, 1.0f);     // #4A5568
                    colors.highlightedColor = new Color(0.38f, 0.43f, 0.52f, 1.0f);
                    colors.pressedColor = new Color(0.22f, 0.25f, 0.32f, 1.0f);
                    colors.disabledColor = new Color(0.20f, 0.20f, 0.20f, 0.5f);
                    break;

                case ButtonStyleType.Danger:
                    // Tactical red alert
                    colors.normalColor = new Color(0.75f, 0.22f, 0.20f, 1.0f);     // #BF3833
                    colors.highlightedColor = new Color(0.85f, 0.28f, 0.25f, 1.0f);
                    colors.pressedColor = new Color(0.60f, 0.18f, 0.16f, 1.0f);
                    colors.disabledColor = new Color(0.35f, 0.20f, 0.20f, 0.5f);
                    break;
            }

            _button.colors = colors;
        }
    }
}
