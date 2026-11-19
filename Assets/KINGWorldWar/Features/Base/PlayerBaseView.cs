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
                _gridView = GetComponentInChildren<BaseGridView>();
                if (_gridView == null) _gridView = gameObject.AddComponent<BaseGridView>();
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
                instance = CreateProceduralBuildingVisual(state.Type, def, worldPos);
            }

            var view = instance.GetComponent<BuildingView>();
            if (view == null) view = instance.AddComponent<BuildingView>();
            view.Initialize(state, def, _loc);
            _views[state.Id] = view;
        }

        private GameObject GetPrefabForType(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.CommandCenter: return _hqPrefab;
                case BuildingType.TrainingCenter: return _barracksPrefab;
                case BuildingType.ResourceDepot: return _depotPrefab;
                case BuildingType.RadarStation: return _radarPrefab;
                case BuildingType.TankFactory: return _factoryPrefab;
                default: return null;
            }
        }

        private GameObject CreateProceduralBuildingVisual(BuildingType type, BuildingDefinition def, Vector3 worldPos)
        {
            var root = new GameObject($"Bld_{type}_{worldPos.x:F0}_{worldPos.z:F0}");
            root.transform.SetParent(_buildingsContainer, false);
            root.transform.position = worldPos;

            float w = def.FootprintWidth * 4f;
            float h = def.FootprintHeight * 4f;

            var basePlat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            basePlat.transform.SetParent(root.transform, false);
            basePlat.transform.localPosition = new Vector3(0, 0.4f, 0);
            basePlat.transform.localScale = new Vector3(w - 0.5f, 0.8f, h - 0.5f);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = GetColorForType(type);
            basePlat.GetComponent<Renderer>().material = mat;

            if (type == BuildingType.Airfield)
            {
                var runway = GameObject.CreatePrimitive(PrimitiveType.Cube);
                runway.transform.SetParent(root.transform, false);
                runway.transform.localPosition = new Vector3(0, 0.85f, 0);
                runway.transform.localScale = new Vector3(w - 1f, 0.1f, 3.5f);
                var runwayMat = new Material(mat) { color = new Color(0.25f, 0.28f, 0.3f) };
                runway.GetComponent<Renderer>().material = runwayMat;

                var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tower.transform.SetParent(root.transform, false);
                tower.transform.localPosition = new Vector3(w * 0.3f, 3.0f, h * 0.3f);
                tower.transform.localScale = new Vector3(2.5f, 3.0f, 2.5f);
                var towerMat = new Material(mat) { color = new Color(0.6f, 0.65f, 0.7f) };
                tower.GetComponent<Renderer>().material = towerMat;
            }
            else if (type == BuildingType.ArmsFactory)
            {
                var hall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hall.transform.SetParent(root.transform, false);
                hall.transform.localPosition = new Vector3(0, 2.0f, 0);
                hall.transform.localScale = new Vector3(w * 0.7f, 3.2f, h * 0.7f);
                var hallMat = new Material(mat) { color = new Color(0.35f, 0.38f, 0.42f) };
                hall.GetComponent<Renderer>().material = hallMat;

                var stack1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stack1.transform.SetParent(root.transform, false);
                stack1.transform.localPosition = new Vector3(-w * 0.25f, 4.5f, h * 0.25f);
                stack1.transform.localScale = new Vector3(1.2f, 2.5f, 1.2f);

                var stack2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stack2.transform.SetParent(root.transform, false);
                stack2.transform.localPosition = new Vector3(-w * 0.25f, 4.5f, -h * 0.25f);
                stack2.transform.localScale = new Vector3(1.2f, 2.5f, 1.2f);
            }
            else if (type == BuildingType.ResearchCenter)
            {
                var lab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lab.transform.SetParent(root.transform, false);
                lab.transform.localPosition = new Vector3(0, 1.5f, 0);
                lab.transform.localScale = new Vector3(w * 0.75f, 2.2f, h * 0.75f);

                var dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dome.transform.SetParent(root.transform, false);
                dome.transform.localPosition = new Vector3(0, 3.0f, 0);
                dome.transform.localScale = new Vector3(3.5f, 3.5f, 3.5f);
                var domeMat = new Material(mat) { color = new Color(0.2f, 0.55f, 0.75f) };
                dome.GetComponent<Renderer>().material = domeMat;
            }
            else
            {
                var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.transform.SetParent(root.transform, false);
                block.transform.localPosition = new Vector3(0, 2f, 0);
                block.transform.localScale = new Vector3(w * 0.8f, 3.5f, h * 0.8f);
            }

            var col = root.GetComponent<BoxCollider>();
            if (col == null) col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 2f, 0);
            col.size = new Vector3(w, 4f, h);

            return root;
        }

        private Color GetColorForType(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.CommandCenter: return new Color(0.25f, 0.35f, 0.25f);
                case BuildingType.Airfield: return new Color(0.3f, 0.35f, 0.4f);
                case BuildingType.ArmsFactory: return new Color(0.45f, 0.3f, 0.25f);
                case BuildingType.ResearchCenter: return new Color(0.2f, 0.4f, 0.55f);
                case BuildingType.ResourceDepot: return new Color(0.55f, 0.45f, 0.2f);
                case BuildingType.TrainingCenter: return new Color(0.35f, 0.45f, 0.35f);
                case BuildingType.TankFactory: return new Color(0.4f, 0.4f, 0.45f);
                default: return new Color(0.4f, 0.4f, 0.4f);
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
