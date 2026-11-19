using System;
using System.Collections.Generic;
using KingWorldWar.Core.Base;
using KingWorldWar.Core.Localization;
using KingWorldWar.Features.Camera;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace KingWorldWar.Features.Base
{
    /// <summary>
    /// Master 3D scene view managing player base structures, raycast selection,
    /// and tactical camera focus synchronization.
    /// </summary>
    public class PlayerBaseView : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private UnityEngine.Camera _worldCamera;
        [SerializeField] private StrategyCameraRig _cameraRig;
        [SerializeField] private BaseGridView _gridView;
        [SerializeField] private Transform _buildingsContainer;

        [Header("Building Prefabs")]
        [SerializeField] private GameObject _hqPrefab;
        [SerializeField] private GameObject _barracksPrefab;
        [SerializeField] private GameObject _depotPrefab;
        [SerializeField] private GameObject _radarPrefab;
        [SerializeField] private GameObject _factoryPrefab;

        private IPlayerBaseService _baseService;
        private ILocalizationService _loc;
        private readonly Dictionary<BuildingId, BuildingView> _views =
            new Dictionary<BuildingId, BuildingView>();

        private BuildingView _selectedView;
        private bool _isDraggingOrPinching;
        private Vector2 _pointerDownPos;

        public IReadOnlyDictionary<BuildingId, BuildingView> BuildingViews => _views;
        public BuildingView SelectedView => _selectedView;
        public BaseGridView GridView => _gridView;

        public void Bind(
            IPlayerBaseService baseService,
            ILocalizationService loc,
            StrategyCameraRig cameraRig = null,
            UnityEngine.Camera worldCamera = null)
        {
            if (_baseService != null)
            {
                _baseService.OnBuildingSelected -= HandleBuildingSelected;
                _baseService.OnUpgradeStarted -= HandleUpgradeStarted;
                _baseService.OnBuildingUpgraded -= HandleBuildingUpgraded;
                _baseService.OnUpgradeCancelled -= HandleUpgradeCancelled;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged -= HandleLanguageChanged;
            }

            _baseService = baseService;
            _loc = loc;
            if (cameraRig != null) _cameraRig = cameraRig;
            if (worldCamera != null) _worldCamera = worldCamera;

            if (_gridView == null)
            {
                _gridView = GetComponentInChildren<BaseGridView>() ?? gameObject.AddComponent<BaseGridView>();
            }

            if (_buildingsContainer == null)
            {
                var container = new GameObject("BuildingsContainer");
                container.transform.SetParent(transform, false);
                _buildingsContainer = container.transform;
            }

            if (_baseService != null)
            {
                _baseService.OnBuildingSelected += HandleBuildingSelected;
                _baseService.OnUpgradeStarted += HandleUpgradeStarted;
                _baseService.OnBuildingUpgraded += HandleBuildingUpgraded;
                _baseService.OnUpgradeCancelled += HandleUpgradeCancelled;
                SpawnAllBuildings();
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged += HandleLanguageChanged;
            }
        }

        public void SetWorldCamera(UnityEngine.Camera camera)
        {
            _worldCamera = camera;
        }

        public void SpawnAllBuildings()
        {
            ClearBuildings();

            if (_baseService == null) return;

            var buildings = _baseService.StateRepository.GetAllBuildings();
            foreach (var b in buildings)
            {
                if (_baseService.DataSource.TryGetDefinition(b.Type, out var def))
                {
                    SpawnBuilding(b, def);
                }
            }
        }

        private void SpawnBuilding(BuildingRuntimeState state, BuildingDefinition def)
        {
            GameObject prefab = GetPrefabForType(state.Type);
            GameObject instance;

            Vector3 worldPos = _gridView.GridToWorld(state.GridX, state.GridY, def.FootprintWidth, def.FootprintHeight);

            if (prefab != null)
            {
                instance = Instantiate(prefab, worldPos, Quaternion.identity, _buildingsContainer);
            }
            else
            {
                instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                instance.transform.SetParent(_buildingsContainer, false);
                instance.transform.position = worldPos;
                instance.transform.localScale = new Vector3(def.FootprintWidth * 4f, 4f, def.FootprintHeight * 4f);
            }

            var view = instance.GetComponent<BuildingView>() ?? instance.AddComponent<BuildingView>();
            view.Initialize(state, def, _loc);
            _views[state.Id] = view;
        }

        private GameObject GetPrefabForType(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.CommandCenter: return _hqPrefab;
                case BuildingType.Barracks: return _barracksPrefab;
                case BuildingType.SupplyDepot: return _depotPrefab;
                case BuildingType.RadarStation: return _radarPrefab;
                case BuildingType.VehicleFactory: return _factoryPrefab;
                default: return null;
            }
        }

        private void ClearBuildings()
        {
            foreach (var v in _views.Values)
            {
                if (v != null && v.gameObject != null)
                {
                    Destroy(v.gameObject);
                }
            }
            _views.Clear();
            _selectedView = null;
        }

        private void Update()
        {
            _baseService?.CheckCompletedUpgrades();
            ProcessSelectionInput();
        }

        private void ProcessSelectionInput()
        {
            if (IsPointerOverUI()) return;

            if (EnhancedTouchSupport.enabled)
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
                var view = hit.collider.GetComponentInParent<BuildingView>();
                if (view != null)
                {
                    SelectBuilding(view.BuildingId);
                    return;
                }
            }

            Deselect();
        }

        public void SelectBuilding(BuildingId id)
        {
            _baseService?.SelectBuilding(id);
        }

        public void Deselect()
        {
            _baseService?.Deselect();
        }

        private void HandleBuildingSelected(BuildingId id)
        {
            if (_selectedView != null)
            {
                _selectedView.SetSelected(false);
                _selectedView = null;
            }

            if (id.IsValid && _views.TryGetValue(id, out var view))
            {
                _selectedView = view;
                _selectedView.SetSelected(true);

                if (_cameraRig != null)
                {
                    _cameraRig.SetFocusPosition(view.CenterPosition);
                }
            }
        }

        private void HandleUpgradeStarted(BuildingRuntimeState state)
        {
            if (_views.TryGetValue(state.Id, out var view))
            {
                view.UpdateState(state);
            }
        }

        private void HandleBuildingUpgraded(BuildingRuntimeState state)
        {
            if (_views.TryGetValue(state.Id, out var view))
            {
                view.UpdateState(state);
            }
        }

        private void HandleUpgradeCancelled(BuildingRuntimeState state)
        {
            if (_views.TryGetValue(state.Id, out var view))
            {
                view.UpdateState(state);
            }
        }

        private void HandleLanguageChanged(Language lang)
        {
            foreach (var view in _views.Values)
            {
                if (view != null) view.UpdateVisuals();
            }
        }

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;
            return EventSystem.current.IsPointerOverGameObject();
        }

        private void OnDestroy()
        {
            if (_baseService != null)
            {
                _baseService.OnBuildingSelected -= HandleBuildingSelected;
                _baseService.OnUpgradeStarted -= HandleUpgradeStarted;
                _baseService.OnBuildingUpgraded -= HandleBuildingUpgraded;
                _baseService.OnUpgradeCancelled -= HandleUpgradeCancelled;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged -= HandleLanguageChanged;
            }
        }
    }
}
