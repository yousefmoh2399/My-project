using System;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.WorldMap;
using TMPro;
using UnityEngine;

namespace KingWorldWar.Features.WorldMap
{
    /// <summary>
    /// 3D presentation view for a single strategic territory on the world map.
    /// Handles visual faction tinting, selection highlighting, and world-space banner.
    /// </summary>
    public class TerritoryView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private MeshRenderer _tileRenderer;
        [SerializeField] private Collider _collider;
        [SerializeField] private GameObject _selectionIndicator;
        [SerializeField] private TextMeshPro _bannerNameText;
        [SerializeField] private TextMeshPro _bannerFactionSymbol;
        [SerializeField] private GameObject _capitalIcon;

        private TerritoryData _data;
        private TerritoryRuntimeState _state;
        private ILocalizationService _loc;
        private MaterialPropertyBlock _propBlock;

        public TerritoryId TerritoryId => _data != null ? _data.Id : TerritoryId.Empty;
        public TerritoryData Data => _data;
        public TerritoryRuntimeState State => _state;
        public Vector3 CenterPosition => transform.position;

        private void Awake()
        {
            if (_collider == null)
            {
                _collider = GetComponent<Collider>();
                if (_collider == null)
                {
                    var box = gameObject.AddComponent<BoxCollider>();
                    box.size = new Vector3(18f, 2f, 18f);
                    _collider = box;
                }
            }

            _propBlock = new MaterialPropertyBlock();
        }

        public void Initialize(TerritoryData data, TerritoryRuntimeState state, ILocalizationService loc)
        {
            _data = data;
            _state = state;
            _loc = loc;

            name = $"Territory_{data.Id.Value}";
            transform.position = data.MapPosition;

            if (_capitalIcon != null)
            {
                _capitalIcon.SetActive(data.Type == TerritoryType.RegionalCapital);
            }

            UpdateVisuals();
            SetSelected(false);
        }

        public void UpdateState(TerritoryRuntimeState state)
        {
            _state = state;
            UpdateVisuals();
        }

        private bool _isSelected;

        public bool IsSelected => _isSelected;
        public TextMeshPro BannerNameText => _bannerNameText;
        public TextMeshPro BannerFactionSymbol => _bannerFactionSymbol;

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            if (_selectionIndicator != null)
            {
                _selectionIndicator.SetActive(selected);
            }
        }

        public void RefreshLocalization()
        {
            UpdateBannerText();
        }

        private void UpdateVisuals()
        {
            UpdateBannerText();
            ApplyFactionColor();
        }

        private void UpdateBannerText()
        {
            if (_data == null) return;

            string displayName = _loc != null ? _loc.GetString(_data.NameKey) : _data.Id.Value;
            if (_bannerNameText != null)
            {
                _bannerNameText.text = displayName;
            }

            if (_bannerFactionSymbol != null && _state != null)
            {
                _bannerFactionSymbol.text = _state.ControllingFaction.GetSymbol();
            }
        }

        private void ApplyFactionColor()
        {
            if (_tileRenderer == null) return;

            Color baseColor = GetFactionColor(_state != null ? _state.ControllingFaction : FactionType.Neutral);

            if (_propBlock == null) _propBlock = new MaterialPropertyBlock();
            _tileRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor("_BaseColor", baseColor);
            _propBlock.SetColor("_Color", baseColor);
            _tileRenderer.SetPropertyBlock(_propBlock);
        }

        public static Color GetFactionColor(FactionType faction)
        {
            switch (faction)
            {
                case FactionType.PlayerAllied:
                    return new Color32(0x3E, 0x6E, 0x3D, 0xFF); // Tactical Green
                case FactionType.HostileRed:
                    return new Color32(0x8B, 0x26, 0x26, 0xFF); // Crimson Red
                case FactionType.HostileBlue:
                    return new Color32(0x2E, 0x4A, 0x6E, 0xFF); // Deep Steel Navy
                case FactionType.Neutral:
                default:
                    return new Color32(0x2B, 0x30, 0x2D, 0xFF); // Neutral Charcoal
            }
        }
    }
}
