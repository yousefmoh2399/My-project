using System;
using KingWorldWar.Core.Army;
using KingWorldWar.Core.Localization;
using TMPro;
using UnityEngine;

namespace KingWorldWar.Features.Army
{
    /// <summary>
    /// 3D presentation view for a military unit archetype in the army inspection/staging ground.
    /// Handles collider raycasting selection, highlight indicator, world-space stats billboard,
    /// and active mobilization progress countdown.
    /// </summary>
    public class ArmyUnitView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Collider _collider;
        [SerializeField] private GameObject _selectionIndicator;
        [SerializeField] private Transform _billboardRoot;
        [SerializeField] private TextMeshPro _nameText;
        [SerializeField] private TextMeshPro _countText;
        [SerializeField] private TextMeshPro _powerText;

        [Header("Training Progress")]
        [SerializeField] private GameObject _trainingProgressRoot;
        [SerializeField] private TextMeshPro _countdownText;
        [SerializeField] private Transform _progressBarFill;

        private UnitDefinition _definition;
        private ILocalizationService _loc;
        private int _mobilizedCount;
        private RecruitmentQueueItem _activeQueue;
        private bool _isSelected;

        public UnitType UnitType => _definition != null ? _definition.Type : UnitType.Infantry;
        public UnitDefinition Definition => _definition;
        public int MobilizedCount => _mobilizedCount;
        public RecruitmentQueueItem ActiveQueue => _activeQueue;
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
                    box.size = new Vector3(6f, 4f, 6f);
                    box.center = new Vector3(0f, 2f, 0f);
                    _collider = box;
                }
            }
        }

        public void Initialize(UnitDefinition def, int count, ILocalizationService loc)
        {
            _definition = def ?? throw new ArgumentNullException(nameof(def));
            _mobilizedCount = Math.Max(0, count);
            _loc = loc;

            name = $"UnitView_{def.Type}";
            EnsureProceduralVisuals();
            UpdateVisuals();
            SetSelected(false);
        }

        public void UpdateCount(int count)
        {
            _mobilizedCount = Math.Max(0, count);
            UpdateVisuals();
        }

        public void SetActiveQueue(RecruitmentQueueItem queue)
        {
            _activeQueue = queue;
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
            if (_definition == null) return;

            string nameStr = _loc != null ? _loc.GetString(_definition.NameKey) : _definition.Type.ToString();
            string countFormat = _loc != null ? _loc.GetString("army.screen.roster_count") : "Mobilized: {0}";
            string countStr = string.Format(countFormat, _mobilizedCount);

            int totalPower = _definition.Stats.CombatPower * _mobilizedCount;
            string powerStr = $"PWR: {totalPower}";

            if (_nameText != null) _nameText.text = nameStr;
            if (_countText != null) _countText.text = countStr;
            if (_powerText != null) _powerText.text = powerStr;

            bool isTraining = _activeQueue != null && !_activeQueue.IsCompleted(DateTime.UtcNow);
            if (_trainingProgressRoot != null)
            {
                _trainingProgressRoot.SetActive(isTraining);
            }

            if (isTraining)
            {
                UpdateTrainingTimer();
            }
        }

        private void Update()
        {
            if (_activeQueue != null)
            {
                if (!_activeQueue.IsCompleted(DateTime.UtcNow))
                {
                    UpdateTrainingTimer();
                }
                else
                {
                    if (_trainingProgressRoot != null && _trainingProgressRoot.activeSelf)
                    {
                        _trainingProgressRoot.SetActive(false);
                    }
                }
            }

            // Keep billboard facing main camera
            if (_billboardRoot != null && UnityEngine.Camera.main != null)
            {
                _billboardRoot.rotation = UnityEngine.Camera.main.transform.rotation;
            }
        }

        private void UpdateTrainingTimer()
        {
            if (_activeQueue == null) return;

            var nowUtc = DateTime.UtcNow;
            var remainingSec = Math.Max(0, (int)_activeQueue.RemainingSeconds(nowUtc));
            var minutes = remainingSec / 60;
            var seconds = remainingSec % 60;

            if (_countdownText != null)
            {
                _countdownText.text = $"{minutes:D2}:{seconds:D2}";
            }

            if (_progressBarFill != null)
            {
                float progress = _activeQueue.ProgressNormalized(nowUtc);
                _progressBarFill.localScale = new Vector3(progress, 1f, 1f);
            }
        }

        private void EnsureProceduralVisuals()
        {
            EnsureCollider();

            // 1. Selection indicator quad
            if (_selectionIndicator == null)
            {
                var indObj = new GameObject("SelectionIndicator");
                indObj.transform.SetParent(transform, false);
                indObj.transform.localPosition = new Vector3(0f, 0.05f, 0f);

                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.SetParent(indObj.transform, false);
                quad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                quad.transform.localScale = new Vector3(8f, 8f, 1f);

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

            // 2. World-Space Billboard
            if (_billboardRoot == null)
            {
                var bbObj = new GameObject("Billboard");
                bbObj.transform.SetParent(transform, false);
                bbObj.transform.localPosition = new Vector3(0f, 4.5f, 0f);
                _billboardRoot = bbObj.transform;

                // Name text
                var nameGo = new GameObject("NameText");
                nameGo.transform.SetParent(_billboardRoot, false);
                nameGo.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                _nameText = nameGo.AddComponent<TextMeshPro>();
                _nameText.fontSize = 4.5f;
                _nameText.alignment = TextAlignmentOptions.Center;
                _nameText.color = Color.white;

                // Mobilized count text
                var countGo = new GameObject("CountText");
                countGo.transform.SetParent(_billboardRoot, false);
                countGo.transform.localPosition = new Vector3(0f, 0.1f, 0f);
                _countText = countGo.AddComponent<TextMeshPro>();
                _countText.fontSize = 3.5f;
                _countText.alignment = TextAlignmentOptions.Center;
                _countText.color = new Color(0.83f, 0.69f, 0.22f, 1f); // Gold

                // Power text
                var powerGo = new GameObject("PowerText");
                powerGo.transform.SetParent(_billboardRoot, false);
                powerGo.transform.localPosition = new Vector3(0f, -0.4f, 0f);
                _powerText = powerGo.AddComponent<TextMeshPro>();
                _powerText.fontSize = 3f;
                _powerText.alignment = TextAlignmentOptions.Center;
                _powerText.color = new Color(0.7f, 0.8f, 0.7f, 1f);

                // Training progress root
                var trainRoot = new GameObject("TrainingProgress");
                trainRoot.transform.SetParent(_billboardRoot, false);
                trainRoot.transform.localPosition = new Vector3(0f, -0.9f, 0f);
                _trainingProgressRoot = trainRoot;

                // Countdown text
                var countDGo = new GameObject("CountdownText");
                countDGo.transform.SetParent(trainRoot.transform, false);
                countDGo.transform.localPosition = new Vector3(0f, 0.3f, 0f);
                _countdownText = countDGo.AddComponent<TextMeshPro>();
                _countdownText.fontSize = 3.5f;
                _countdownText.alignment = TextAlignmentOptions.Center;
                _countdownText.color = new Color(1f, 0.85f, 0.3f, 1f);

                // Progress bar bg
                var barBg = GameObject.CreatePrimitive(PrimitiveType.Quad);
                barBg.name = "ProgressBarBg";
                barBg.transform.SetParent(trainRoot.transform, false);
                barBg.transform.localPosition = new Vector3(0f, 0f, 0f);
                barBg.transform.localScale = new Vector3(4f, 0.3f, 1f);
                var barBgCollider = barBg.GetComponent<Collider>();
                if (barBgCollider != null) DestroyImmediate(barBgCollider);
                var barBgRenderer = barBg.GetComponent<MeshRenderer>();
                if (barBgRenderer != null)
                {
                    var bgMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
                    bgMat.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                    barBgRenderer.material = bgMat;
                }

                // Progress bar fill
                var barFill = GameObject.CreatePrimitive(PrimitiveType.Quad);
                barFill.name = "ProgressBarFill";
                barFill.transform.SetParent(barBg.transform, false);
                barFill.transform.localPosition = new Vector3(-0.5f, 0f, -0.01f);
                barFill.transform.localScale = new Vector3(0f, 1f, 1f);
                var barFillCollider = barFill.GetComponent<Collider>();
                if (barFillCollider != null) DestroyImmediate(barFillCollider);
                var barFillRenderer = barFill.GetComponent<MeshRenderer>();
                if (barFillRenderer != null)
                {
                    var fillMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
                    fillMat.color = new Color(0.2f, 0.8f, 0.4f, 1f); // Green
                    barFillRenderer.material = fillMat;
                }
                _progressBarFill = barFill.transform;

                trainRoot.SetActive(false);
            }
        }
    }
}
