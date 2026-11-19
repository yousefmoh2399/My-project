using System;
using KingWorldWar.Core.Base;
using KingWorldWar.Core.Localization;
using TMPro;
using UnityEngine;

namespace KingWorldWar.Features.Base
{
    /// <summary>
    /// 3D presentation view for a single building structure in the player base.
    /// Handles collider raycast selection, visual selection indicator,
    /// world-space level banner, and real-time upgrade countdown timer.
    /// </summary>
    public class BuildingView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Collider _collider;
        [SerializeField] private GameObject _selectionIndicator;
        [SerializeField] private Transform _billboardRoot;
        [SerializeField] private TextMeshPro _nameText;
        [SerializeField] private TextMeshPro _levelText;

        [Header("Upgrade Progress")]
        [SerializeField] private GameObject _upgradeProgressRoot;
        [SerializeField] private TextMeshPro _countdownText;
        [SerializeField] private Transform _progressBarFill;

        private BuildingRuntimeState _state;
        private BuildingDefinition _definition;
        private ILocalizationService _loc;
        private bool _isSelected;

        public BuildingId BuildingId => _state != null ? _state.Id : BuildingId.Empty;
        public BuildingType BuildingType => _state != null ? _state.Type : BuildingType.CommandCenter;
        public BuildingRuntimeState State => _state;
        public BuildingDefinition Definition => _definition;
        public bool IsSelected => _isSelected;
        public Vector3 CenterPosition => transform.position;

        private void Awake()
        {
            EnsureCollider();
        }

        private void EnsureCollider()
        {
            if (_collider == null)
            {
                _collider = GetComponent<Collider>();
                if (_collider == null)
                {
                    var box = gameObject.AddComponent<BoxCollider>();
                    box.size = new Vector3(8f, 6f, 8f);
                    box.center = new Vector3(0f, 3f, 0f);
                    _collider = box;
                }
            }
        }

        public void Initialize(BuildingRuntimeState state, BuildingDefinition def, ILocalizationService loc)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _definition = def ?? throw new ArgumentNullException(nameof(def));
            _loc = loc;

            name = $"Building_{state.Id.Value}";
            EnsureProceduralVisuals();
            UpdateVisuals();
            SetSelected(false);
        }

        public void UpdateState(BuildingRuntimeState state)
        {
            _state = state;
            UpdateVisuals();
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            if (_selectionIndicator != null)
            {
                _selectionIndicator.SetActive(selected);
            }
        }

        public void UpdateVisuals()
        {
            if (_state == null || _definition == null) return;

            string nameStr = _loc != null ? _loc.GetString(_definition.NameLocalizationKey) : _definition.Type.ToString();
            string levelFormat = _loc != null ? _loc.GetString("base.drawer.level") : "Level {0}";
            string levelStr = string.Format(levelFormat, _state.CurrentLevel);

            if (_nameText != null) _nameText.text = nameStr;
            if (_levelText != null) _levelText.text = levelStr;

            bool isUpgrading = _state.IsUpgrading;
            if (_upgradeProgressRoot != null)
            {
                _upgradeProgressRoot.SetActive(isUpgrading);
            }

            if (isUpgrading)
            {
                UpdateUpgradeTimer();
            }
        }

        private void Update()
        {
            if (_state != null && _state.IsUpgrading)
            {
                UpdateUpgradeTimer();
            }
        }

        private void UpdateUpgradeTimer()
        {
            if (_state == null || !_state.IsUpgrading) return;

            var remaining = _state.GetRemainingUpgradeTime(DateTime.UtcNow);
            if (_countdownText != null)
            {
                _countdownText.text = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
            }

            if (_progressBarFill != null)
            {
                float progress = _state.GetUpgradeProgress(DateTime.UtcNow);
                _progressBarFill.localScale = new Vector3(progress, 1f, 1f);
            }
        }

        private void EnsureProceduralVisuals()
        {
            EnsureCollider();

            // 1. Selection indicator
            if (_selectionIndicator == null)
            {
                var indObj = new GameObject("SelectionIndicator");
                indObj.transform.SetParent(transform, false);
                indObj.transform.localPosition = new Vector3(0f, 0.05f, 0f);

                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.SetParent(indObj.transform, false);
                quad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                float sizeW = _definition != null ? _definition.FootprintWidth * 4f : 12f;
                float sizeH = _definition != null ? _definition.FootprintHeight * 4f : 12f;
                quad.transform.localScale = new Vector3(sizeW, sizeH, 1f);

                var quadCollider = quad.GetComponent<Collider>();
                if (quadCollider != null) DestroyImmediate(quadCollider);

                var renderer = quad.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
                    mat.color = new Color(0.83f, 0.69f, 0.22f, 0.5f); // Restrained Gold
                    renderer.material = mat;
                }

                _selectionIndicator = indObj;
            }

            // 2. Billboard root
            if (_billboardRoot == null)
            {
                var bbObj = new GameObject("BillboardRoot");
                bbObj.transform.SetParent(transform, false);
                bbObj.transform.localPosition = new Vector3(0f, 6.5f, 0f);
                bbObj.transform.localRotation = Quaternion.Euler(45f, 0f, 0f); // Tilted towards camera
                _billboardRoot = bbObj.transform;

                // Name label
                var nameObj = new GameObject("NameText");
                nameObj.transform.SetParent(_billboardRoot, false);
                nameObj.transform.localPosition = new Vector3(0f, 0.8f, 0f);
                _nameText = nameObj.AddComponent<TextMeshPro>();
                _nameText.fontSize = 7;
                _nameText.alignment = TextAlignmentOptions.Center;
                _nameText.color = Color.white;

                // Level label
                var lvlObj = new GameObject("LevelText");
                lvlObj.transform.SetParent(_billboardRoot, false);
                lvlObj.transform.localPosition = new Vector3(0f, 0f, 0f);
                _levelText = lvlObj.AddComponent<TextMeshPro>();
                _levelText.fontSize = 6;
                _levelText.alignment = TextAlignmentOptions.Center;
                _levelText.color = new Color(0.83f, 0.69f, 0.22f); // Gold

                // Upgrade root
                var upObj = new GameObject("UpgradeProgress");
                upObj.transform.SetParent(_billboardRoot, false);
                upObj.transform.localPosition = new Vector3(0f, -0.8f, 0f);
                _upgradeProgressRoot = upObj;

                // Countdown label
                var cdObj = new GameObject("CountdownText");
                cdObj.transform.SetParent(_upgradeProgressRoot.transform, false);
                cdObj.transform.localPosition = new Vector3(0f, 0f, 0f);
                _countdownText = cdObj.AddComponent<TextMeshPro>();
                _countdownText.fontSize = 5.5f;
                _countdownText.alignment = TextAlignmentOptions.Center;
                _countdownText.color = new Color(0.2f, 0.9f, 0.3f); // Green

                _upgradeProgressRoot.SetActive(false);
            }
        }
    }
}
