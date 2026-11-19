using KingWorldWar.Core.UI;
using TMPro;
using UnityEngine;

namespace KingWorldWar.UI.Common
{
    /// <summary>
    /// Displays non-authoritative client readout of primary game resources (Gold, Oil, Iron).
    /// </summary>
    public class ResourceHeaderView : MonoBehaviour
    {
        [Header("Text Displays")]
        [SerializeField] private TextMeshProUGUI _goldText;
        [SerializeField] private TextMeshProUGUI _oilText;
        [SerializeField] private TextMeshProUGUI _ironText;

        private IResourceState _state;

        public void Bind(IResourceState state)
        {
            if (_state != null)
            {
                _state.OnResourcesChanged -= UpdateDisplay;
            }

            _state = state;

            if (_state != null)
            {
                _state.OnResourcesChanged += UpdateDisplay;
            }

            UpdateDisplay();
        }

        private void OnDestroy()
        {
            if (_state != null)
            {
                _state.OnResourcesChanged -= UpdateDisplay;
            }
        }

        public void SetManualValues(long gold, long oil, long iron)
        {
            if (_goldText != null) _goldText.text = gold.ToString("N0");
            if (_oilText != null) _oilText.text = oil.ToString("N0");
            if (_ironText != null) _ironText.text = iron.ToString("N0");
        }

        private void UpdateDisplay()
        {
            if (_state == null) return;
            SetManualValues(_state.Gold, _state.Oil, _state.Iron);
        }
    }
}
