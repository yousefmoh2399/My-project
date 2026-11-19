using System;
using System.Collections.Generic;
using KingWorldWar.Core.Army;
using KingWorldWar.Core.Localization;
using KingWorldWar.Features.Camera;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace KingWorldWar.Features.Army
{
    /// <summary>
    /// Tactical 3D staging ground and military parade inspector for KING WORLD WAR.
    /// Spawns 3D formations of all 4 unit archetypes, processes raycast selection,
    /// and synchronizes military recruitment status in real time.
    /// </summary>
    public class PlayerArmyView : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private StrategyCameraRig _cameraRig;
        [SerializeField] private UnityEngine.Camera _worldCamera;
        [SerializeField] private Transform _unitsContainer;

        [Header("Unit Archetype Prefabs")]
        [SerializeField] private GameObject _infantryPrefab;
        [SerializeField] private GameObject _reconPrefab;
        [SerializeField] private GameObject _tankPrefab;
        [SerializeField] private GameObject _artilleryPrefab;

        private IArmyService _armyService;
        private ILocalizationService _loc;
        private readonly Dictionary<UnitType, ArmyUnitView> _unitViews = new Dictionary<UnitType, ArmyUnitView>();
        private ArmyUnitView _selectedView;
        private Vector2 _pointerDownPos;
        private bool _isDraggingOrPinching;

        public event Action<ArmyUnitView> OnUnitSelected;
        public event Action OnUnitDeselected;

        public IReadOnlyDictionary<UnitType, ArmyUnitView> UnitViews => _unitViews;
        public ArmyUnitView SelectedView => _selectedView;
        public Vector3 InspectionCenter => transform.position;

        private void OnEnable()
        {
            if (!EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Enable();
            }
        }

        public void Initialize(
            IArmyService armyService,
            ILocalizationService loc,
            StrategyCameraRig cameraRig = null,
            UnityEngine.Camera worldCamera = null)
        {
            if (_armyService != null)
            {
                _armyService.OnRosterUpdated -= HandleRosterUpdated;
                _armyService.OnRecruitmentStarted -= HandleRecruitmentStarted;
                _armyService.OnRecruitmentCancelled -= HandleRecruitmentCancelled;
                _armyService.OnRecruitmentCompleted -= HandleRecruitmentCompleted;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged -= HandleLanguageChanged;
            }

            _armyService = armyService;
            _loc = loc;
            if (cameraRig != null) _cameraRig = cameraRig;
            if (worldCamera != null) _worldCamera = worldCamera;

            if (_unitsContainer == null)
            {
                var container = new GameObject("UnitsContainer");
                container.transform.SetParent(transform, false);
                _unitsContainer = container.transform;
            }

            EnsureParadeGroundPlatform();
            SpawnAllUnitArchetypes();

            if (_armyService != null)
            {
                _armyService.OnRosterUpdated += HandleRosterUpdated;
                _armyService.OnRecruitmentStarted += HandleRecruitmentStarted;
                _armyService.OnRecruitmentCancelled += HandleRecruitmentCancelled;
                _armyService.OnRecruitmentCompleted += HandleRecruitmentCompleted;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged += HandleLanguageChanged;
            }
        }

        private void OnDestroy()
        {
            if (_armyService != null)
            {
                _armyService.OnRosterUpdated -= HandleRosterUpdated;
                _armyService.OnRecruitmentStarted -= HandleRecruitmentStarted;
                _armyService.OnRecruitmentCancelled -= HandleRecruitmentCancelled;
                _armyService.OnRecruitmentCompleted -= HandleRecruitmentCompleted;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged -= HandleLanguageChanged;
            }
        }

        private void Update()
        {
            HandlePointerInput();
        }

        private void HandlePointerInput()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (Touch.activeTouches.Count > 0)
            {
                var touches = Touch.activeTouches;
                if (touches.Count == 1)
                {
                    var touch = touches[0];
                    if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                    {
                        _pointerDownPos = touch.screenPosition;
                        _isDraggingOrPinching = false;
                    }
                    else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
                    {
                        if (Vector2.Distance(_pointerDownPos, touch.screenPosition) > 15f)
                        {
                            _isDraggingOrPinching = true;
                        }
                    }
                    else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended && !_isDraggingOrPinching)
                    {
                        PerformRaycastSelection(touch.screenPosition);
                    }
                }
                else if (touches.Count > 1)
                {
                    _isDraggingOrPinching = true;
                }
            }

            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame)
                {
                    _pointerDownPos = mouse.position.ReadValue();
                    _isDraggingOrPinching = false;
                }
                else if (mouse.leftButton.isPressed)
                {
                    if (Vector2.Distance(_pointerDownPos, mouse.position.ReadValue()) > 15f)
                    {
                        _isDraggingOrPinching = true;
                    }
                }
                else if (mouse.leftButton.wasReleasedThisFrame && !_isDraggingOrPinching)
                {
                    PerformRaycastSelection(mouse.position.ReadValue());
                }
            }
        }

        public void PerformRaycastSelection(Vector2 screenPosition)
        {
            if (_worldCamera == null) _worldCamera = UnityEngine.Camera.main;
            if (_worldCamera == null) return;

            Ray ray = _worldCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 500f))
            {
                var view = hit.collider.GetComponentInParent<ArmyUnitView>();
                if (view != null)
                {
                    SelectUnit(view.UnitType);
                    return;
                }
            }

            Deselect();
        }

        public void SelectUnit(UnitType type)
        {
            if (_selectedView != null)
            {
                _selectedView.SetSelected(false);
            }

            if (_unitViews.TryGetValue(type, out var view))
            {
                _selectedView = view;
                view.SetSelected(true);

                if (_cameraRig != null)
                {
                    _cameraRig.SetFocusPosition(new Vector2(view.CenterPosition.x, view.CenterPosition.z));
                }

                OnUnitSelected?.Invoke(view);
            }
            else
            {
                _selectedView = null;
                OnUnitDeselected?.Invoke();
            }
        }

        public void Deselect()
        {
            if (_selectedView != null)
            {
                _selectedView.SetSelected(false);
                _selectedView = null;
                OnUnitDeselected?.Invoke();
            }
        }

        public void SpawnAllUnitArchetypes()
        {
            if (_armyService == null || _unitsContainer == null) return;

            var ds = _armyService.DataSource;
            var roster = _armyService.GetRoster();
            var queues = _armyService.GetActiveQueues();

            // Archetype layout on inspection grounds (relative to base transform at 400,0,0)
            var offsets = new Dictionary<UnitType, Vector3>
            {
                { UnitType.Infantry, new Vector3(-12f, 0f, 8f) },
                { UnitType.ReconVehicle, new Vector3(12f, 0f, 8f) },
                { UnitType.Tank, new Vector3(-12f, 0f, -8f) },
                { UnitType.Artillery, new Vector3(12f, 0f, -8f) }
            };

            foreach (var kvp in offsets)
            {
                var unitType = kvp.Key;
                var localPos = kvp.Value;

                if (!ds.TryGetDefinition(unitType, out var def)) continue;

                if (_unitViews.TryGetValue(unitType, out var existingView) && existingView != null)
                {
                    existingView.UpdateCount(roster.GetCount(unitType));
                    continue;
                }

                GameObject prefab = GetPrefabForType(unitType);
                GameObject instance;

                if (prefab != null)
                {
                    instance = Instantiate(prefab, _unitsContainer);
                }
                else
                {
                    instance = CreateProceduralUnitModel(unitType);
                    instance.transform.SetParent(_unitsContainer, false);
                }

                instance.transform.localPosition = localPos;
                var view = instance.GetComponent<ArmyUnitView>();
                if (view == null) view = instance.AddComponent<ArmyUnitView>();
                view.Initialize(def, roster.GetCount(unitType), _loc);

                // Check active queue
                RecruitmentQueueItem activeQueue = null;
                for (var q = 0; q < queues.Count; q++)
                {
                    if (queues[q].Type == unitType)
                    {
                        activeQueue = queues[q];
                        break;
                    }
                }
                view.SetActiveQueue(activeQueue);

                _unitViews[unitType] = view;
            }
        }

        private GameObject GetPrefabForType(UnitType type)
        {
            return type switch
            {
                UnitType.Infantry => _infantryPrefab,
                UnitType.ReconVehicle => _reconPrefab,
                UnitType.Tank => _tankPrefab,
                UnitType.Artillery => _artilleryPrefab,
                _ => null
            };
        }

        private GameObject CreateProceduralUnitModel(UnitType type)
        {
            var root = new GameObject($"Model_{type}");

            // Tactical military colors
            Color bodyColor = type switch
            {
                UnitType.Infantry => new Color(0.24f, 0.31f, 0.24f), // Muted Olive Green
                UnitType.ReconVehicle => new Color(0.35f, 0.40f, 0.35f), // Steel Olive
                UnitType.Tank => new Color(0.20f, 0.22f, 0.20f), // Charcoal Armor
                UnitType.Artillery => new Color(0.28f, 0.28f, 0.30f), // Gunmetal Gray
                _ => Color.gray
            };

            var chassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chassis.transform.SetParent(root.transform, false);

            if (type == UnitType.Infantry)
            {
                // Squad of 3 soldiers (capsules)
                DestroyImmediate(chassis);
                for (int i = -1; i <= 1; i++)
                {
                    var soldier = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    soldier.name = $"Soldier_{i}";
                    soldier.transform.SetParent(root.transform, false);
                    soldier.transform.localPosition = new Vector3(i * 1.5f, 1f, 0f);
                    soldier.transform.localScale = new Vector3(0.8f, 1f, 0.8f);

                    var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                    mat.color = bodyColor;
                    soldier.GetComponent<MeshRenderer>().material = mat;
                }
            }
            else if (type == UnitType.Tank)
            {
                chassis.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                chassis.transform.localScale = new Vector3(3.2f, 1.0f, 4.5f);

                var turret = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                turret.transform.SetParent(root.transform, false);
                turret.transform.localPosition = new Vector3(0f, 1.4f, -0.2f);
                turret.transform.localScale = new Vector3(2.0f, 0.5f, 2.0f);

                var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                barrel.transform.SetParent(turret.transform, false);
                barrel.transform.localPosition = new Vector3(0f, 0.2f, 1.8f);
                barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                barrel.transform.localScale = new Vector3(0.25f, 1.8f, 0.25f);

                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.color = bodyColor;
                chassis.GetComponent<MeshRenderer>().material = mat;
                turret.GetComponent<MeshRenderer>().material = mat;
                barrel.GetComponent<MeshRenderer>().material = mat;
            }
            else if (type == UnitType.Artillery)
            {
                chassis.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                chassis.transform.localScale = new Vector3(3.0f, 1.0f, 4.2f);

                var gun = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                gun.transform.SetParent(root.transform, false);
                gun.transform.localPosition = new Vector3(0f, 1.8f, 0.5f);
                gun.transform.localRotation = Quaternion.Euler(60f, 0f, 0f); // Angled upward
                gun.transform.localScale = new Vector3(0.35f, 2.5f, 0.35f);

                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.color = bodyColor;
                chassis.GetComponent<MeshRenderer>().material = mat;
                gun.GetComponent<MeshRenderer>().material = mat;
            }
            else // ReconVehicle
            {
                chassis.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                chassis.transform.localScale = new Vector3(2.4f, 0.9f, 3.8f);

                var antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                antenna.transform.SetParent(root.transform, false);
                antenna.transform.localPosition = new Vector3(0.8f, 1.6f, -1.0f);
                antenna.transform.localScale = new Vector3(0.08f, 1.2f, 0.08f);

                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.color = bodyColor;
                chassis.GetComponent<MeshRenderer>().material = mat;
                antenna.GetComponent<MeshRenderer>().material = mat;
            }

            return root;
        }

        private void EnsureParadeGroundPlatform()
        {
            var existingTarmac = transform.Find("ParadeTarmac");
            if (existingTarmac != null) return;

            var tarmac = GameObject.CreatePrimitive(PrimitiveType.Quad);
            tarmac.name = "ParadeTarmac";
            tarmac.transform.SetParent(transform, false);
            tarmac.transform.localPosition = new Vector3(0f, 0.01f, 0f);
            tarmac.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            tarmac.transform.localScale = new Vector3(60f, 40f, 1f);

            var col = tarmac.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);

            var renderer = tarmac.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.color = new Color(0.12f, 0.14f, 0.13f); // Dark Charcoal Military Tarmac
                renderer.material = mat;
            }
        }

        private void HandleRosterUpdated()
        {
            if (_armyService == null) return;
            var roster = _armyService.GetRoster();
            foreach (var kvp in _unitViews)
            {
                kvp.Value.UpdateCount(roster.GetCount(kvp.Key));
            }
        }

        private void HandleRecruitmentStarted(RecruitmentQueueItem item)
        {
            if (_unitViews.TryGetValue(item.Type, out var view))
            {
                view.SetActiveQueue(item);
            }
        }

        private void HandleRecruitmentCancelled(string queueId)
        {
            foreach (var kvp in _unitViews)
            {
                if (kvp.Value.ActiveQueue != null && kvp.Value.ActiveQueue.QueueId == queueId)
                {
                    kvp.Value.SetActiveQueue(null);
                }
            }
        }

        private void HandleRecruitmentCompleted(UnitType type, int count)
        {
            if (_unitViews.TryGetValue(type, out var view))
            {
                view.SetActiveQueue(null);
            }
            HandleRosterUpdated();
        }

        private void HandleLanguageChanged(Language language)
        {
            foreach (var kvp in _unitViews)
            {
                kvp.Value.UpdateVisuals();
            }
        }
    }
}
