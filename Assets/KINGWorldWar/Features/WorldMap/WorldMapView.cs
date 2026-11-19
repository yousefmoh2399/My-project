using System;
using System.Collections.Generic;
using KingWorldWar.Core.Localization;
using KingWorldWar.Core.WorldMap;
using KingWorldWar.Features.Camera;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace KingWorldWar.Features.WorldMap
{
    /// <summary>
    /// Master 3D scene view managing world map territory views, raycast selection,
    /// and tactical camera focus synchronization.
    /// </summary>
    public class WorldMapView : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private UnityEngine.Camera _worldCamera;
        [SerializeField] private StrategyCameraRig _cameraRig;
        [SerializeField] private Transform _territoriesContainer;

        [Header("Prefabs & Templates")]
        [SerializeField] private TerritoryView _territoryPrefab;

        private IWorldMapService _mapService;
        private ILocalizationService _loc;
        private readonly Dictionary<TerritoryId, TerritoryView> _views =
            new Dictionary<TerritoryId, TerritoryView>();

        private TerritoryView _selectedView;
        private bool _isDraggingOrPinching;
        private Vector2 _pointerDownPos;

        public IReadOnlyDictionary<TerritoryId, TerritoryView> TerritoryViews => _views;
        public TerritoryView SelectedView => _selectedView;

        public void Bind(IWorldMapService mapService, ILocalizationService loc, StrategyCameraRig cameraRig = null, UnityEngine.Camera worldCamera = null)
        {
            if (_mapService != null)
            {
                _mapService.OnTerritorySelected -= HandleTerritorySelected;
                _mapService.StateRepository.OnTerritoryStateChanged -= HandleStateChanged;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged -= HandleLanguageChanged;
            }

            _mapService = mapService;
            _loc = loc;
            if (cameraRig != null) _cameraRig = cameraRig;

            if (worldCamera != null)
            {
                _worldCamera = worldCamera;
            }
            else if (_cameraRig != null && _cameraRig.GetComponentInChildren<UnityEngine.Camera>() != null)
            {
                _worldCamera = _cameraRig.GetComponentInChildren<UnityEngine.Camera>();
            }
            else if (_worldCamera == null)
            {
                _worldCamera = UnityEngine.Camera.main;
            }

            if (_mapService != null)
            {
                _mapService.OnTerritorySelected += HandleTerritorySelected;
                _mapService.StateRepository.OnTerritoryStateChanged += HandleStateChanged;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged += HandleLanguageChanged;
            }

            SpawnOrUpdateTerritoryViews();
        }

        public void SetWorldCamera(UnityEngine.Camera camera)
        {
            _worldCamera = camera;
        }

        private void OnEnable()
        {
            if (!EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Enable();
            }
        }

        private void OnDisable()
        {
            if (_mapService != null)
            {
                _mapService.OnTerritorySelected -= HandleTerritorySelected;
                _mapService.StateRepository.OnTerritoryStateChanged -= HandleStateChanged;
            }

            if (_loc != null)
            {
                _loc.OnLanguageChanged -= HandleLanguageChanged;
            }
        }

        public void SpawnOrUpdateTerritoryViews()
        {
            if (_mapService == null) return;

            var territories = _mapService.DataSource.GetTerritories();
            var container = _territoriesContainer != null ? _territoriesContainer : transform;

            // If views are already present as children in scene (e.g. from editor builder)
            var existingViews = container.GetComponentsInChildren<TerritoryView>(true);
            foreach (var ev in existingViews)
            {
                if (ev != null && ev.Data != null && ev.Data.Id.IsValid)
                {
                    _views[ev.Data.Id] = ev;
                }
            }

            foreach (var t in territories)
            {
                if (!_views.TryGetValue(t.Id, out var view))
                {
                    if (_territoryPrefab != null)
                    {
                        view = Instantiate(_territoryPrefab, container);
                    }
                    else
                    {
                        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        go.transform.SetParent(container);
                        go.transform.localScale = new Vector3(18f, 1.5f, 18f);
                        view = go.AddComponent<TerritoryView>();
                    }

                    _views[t.Id] = view;
                }

                var state = _mapService.StateRepository.GetState(t.Id);
                view.Initialize(t, state, _loc);
            }
        }

        private void Update()
        {
            ProcessSelectionInput();
        }

        private void ProcessSelectionInput()
        {
            // Do not raycast through active UI elements
            if (IsPointerOverUI()) return;

            // Handle touch
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

            // Handle mouse
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
                var territoryView = hit.collider.GetComponentInParent<TerritoryView>();
                if (territoryView != null)
                {
                    _mapService?.SelectTerritory(territoryView.TerritoryId);
                    return;
                }
            }

            // Tapped on empty terrain / background -> Deselect
            _mapService?.SelectTerritory(null);
        }

        private void HandleTerritorySelected(TerritoryId? id)
        {
            if (_selectedView != null)
            {
                _selectedView.SetSelected(false);
                _selectedView = null;
            }

            if (id.HasValue && _views.TryGetValue(id.Value, out var view))
            {
                _selectedView = view;
                _selectedView.SetSelected(true);

                if (_cameraRig != null)
                {
                    _cameraRig.SetFocusPosition(view.CenterPosition, immediate: false);
                }
            }
        }

        private void HandleStateChanged(TerritoryId id, TerritoryRuntimeState state)
        {
            if (_views.TryGetValue(id, out var view))
            {
                view.UpdateState(state);
            }
        }

        private void HandleLanguageChanged(Language lang)
        {
            foreach (var kvp in _views)
            {
                kvp.Value.RefreshLocalization();
            }
        }

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;
            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}
