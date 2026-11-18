using KingWorldWar.Core.Camera;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace KingWorldWar.Features.Camera
{
    /// <summary>
    /// Production RTS/strategy camera rig supporting desktop mouse and mobile multi-touch gestures,
    /// bounded movement, smooth damping, and UI pointer filtering.
    /// </summary>
    public class StrategyCameraRig : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CameraConfigSO _config;
        [SerializeField] private UnityEngine.Camera _targetCamera;

        [Header("Runtime State")]
        [SerializeField] private Vector3 _targetPosition;
        [SerializeField] private float _targetZoom;

        private Vector3 _currentPosition;
        private float _currentZoom;
        private Vector3 _positionVelocity;
        private float _zoomVelocity;

        private bool _isDragging;
        private Vector2 _lastPointerPosition;
        private bool _pointerDownOverUI;

        public CameraConfigSO Config
        {
            get => _config;
            set
            {
                _config = value;
                ApplyConfigDefaults();
            }
        }

        public UnityEngine.Camera TargetCamera
        {
            get => _targetCamera;
            set => _targetCamera = value;
        }

        public Vector3 FocusPosition => _currentPosition;
        public Vector3 TargetFocusPosition => _targetPosition;
        public float CurrentZoom => _currentZoom;
        public float TargetZoom => _targetZoom;

        private void Awake()
        {
            if (_config == null)
            {
                _config = CameraConfigSO.CreateDefaultInstance();
            }

            if (_targetCamera == null)
            {
                _targetCamera = GetComponentInChildren<UnityEngine.Camera>();
                if (_targetCamera == null)
                {
                    var camGo = new GameObject("MainCamera");
                    camGo.transform.SetParent(transform);
                    _targetCamera = camGo.AddComponent<UnityEngine.Camera>();
                    camGo.tag = "MainCamera";
                }
            }

            ApplyConfigDefaults();
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
            if (EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Disable();
            }
            ResetInputState();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                ResetInputState();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                ResetInputState();
            }
        }

        private void ApplyConfigDefaults()
        {
            if (_config == null) return;

            _targetPosition = CameraMath.ClampPosition(transform.position, _config.MinBounds, _config.MaxBounds);
            _currentPosition = _targetPosition;
            _targetZoom = CameraMath.ClampZoom(_config.DefaultZoom, _config.MinZoom, _config.MaxZoom);
            _currentZoom = _targetZoom;

            UpdateCameraTransformImmediate();
        }

        public void SetFocusPosition(Vector3 position, bool immediate = false)
        {
            if (_config == null) return;

            _targetPosition = CameraMath.ClampPosition(position, _config.MinBounds, _config.MaxBounds);
            if (immediate)
            {
                _currentPosition = _targetPosition;
                _positionVelocity = Vector3.zero;
                UpdateCameraTransformImmediate();
            }
        }

        public void SetZoom(float zoom, bool immediate = false)
        {
            if (_config == null) return;

            _targetZoom = CameraMath.ClampZoom(zoom, _config.MinZoom, _config.MaxZoom);
            if (immediate)
            {
                _currentZoom = _targetZoom;
                _zoomVelocity = 0f;
                UpdateCameraTransformImmediate();
            }
        }

        private void Update()
        {
            ProcessInput();
            UpdateMovement(Time.unscaledDeltaTime);
        }

        public void UpdateMovement(float deltaTime)
        {
            if (_config == null || deltaTime <= 0f) return;

            _currentPosition = CameraMath.SmoothDampPosition(
                _currentPosition,
                _targetPosition,
                ref _positionVelocity,
                _config.PositionSmoothTime,
                deltaTime);

            _currentZoom = CameraMath.SmoothDampZoom(
                _currentZoom,
                _targetZoom,
                ref _zoomVelocity,
                _config.ZoomSmoothTime,
                deltaTime);

            UpdateCameraTransformImmediate();
        }

        private void UpdateCameraTransformImmediate()
        {
            transform.position = _currentPosition;

            if (_targetCamera != null && _config != null)
            {
                float pitchRad = _config.PitchAngle * Mathf.Deg2Rad;
                float height = _currentZoom * Mathf.Sin(pitchRad);
                float distance = _currentZoom * Mathf.Cos(pitchRad);

                _targetCamera.transform.localPosition = new Vector3(0f, height, -distance);
                _targetCamera.transform.localRotation = Quaternion.Euler(_config.PitchAngle, 0f, 0f);
            }
        }

        private void ProcessInput()
        {
            if (_config == null) return;

            // Touch input priority (multi-touch handling)
            if (EnhancedTouchSupport.enabled && Touch.activeTouches.Count > 0)
            {
                ProcessTouchInput(Touch.activeTouches);
                return;
            }

            // Fallback to desktop Mouse input
            ProcessMouseInput();
        }

        private void ProcessTouchInput(ReadOnlyArray<Touch> touches)
        {
            if (touches.Count == 1)
            {
                var touch = touches[0];

                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    _pointerDownOverUI = IsPointerOverUI(touch.touchId);
                    _lastPointerPosition = touch.screenPosition;
                    _isDragging = !_pointerDownOverUI;
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved && _isDragging)
                {
                    Vector2 delta = touch.screenPosition - _lastPointerPosition;
                    _lastPointerPosition = touch.screenPosition;
                    ApplyPanDelta(delta);
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    ResetInputState();
                }
            }
            else if (touches.Count >= 2)
            {
                // Two-finger pinch zoom
                var t0 = touches[0];
                var t1 = touches[1];

                if (!_pointerDownOverUI)
                {
                    Vector2 t0Prev = t0.screenPosition - t0.delta;
                    Vector2 t1Prev = t1.screenPosition - t1.delta;

                    float pinchRatio = CameraMath.CalculatePinchRatio(t0Prev, t0.screenPosition, t1Prev, t1.screenPosition);
                    if (Mathf.Abs(pinchRatio - 1.0f) > 0.001f)
                    {
                        // pinchRatio > 1 means fingers spread apart -> zoom in (reduce distance)
                        float zoomDelta = (1.0f - pinchRatio) * _config.PinchSensitivity;
                        _targetZoom = CameraMath.ClampZoom(_targetZoom + zoomDelta, _config.MinZoom, _config.MaxZoom);
                    }
                }
            }
        }

        private void ProcessMouseInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            // Scroll zoom
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f && !IsPointerOverUI(-1))
            {
                float zoomDelta = -Mathf.Sign(scroll) * _config.MouseScrollSensitivity;
                _targetZoom = CameraMath.ClampZoom(_targetZoom + zoomDelta, _config.MinZoom, _config.MaxZoom);
            }

            // Mouse Drag pan (Right click or Middle click or Left click)
            bool isPressing = mouse.leftButton.isPressed || mouse.rightButton.isPressed || mouse.middleButton.isPressed;
            Vector2 currentMousePos = mouse.position.ReadValue();

            if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame)
            {
                _pointerDownOverUI = IsPointerOverUI(-1);
                _lastPointerPosition = currentMousePos;
                _isDragging = !_pointerDownOverUI;
            }
            else if (isPressing && _isDragging)
            {
                Vector2 delta = currentMousePos - _lastPointerPosition;
                _lastPointerPosition = currentMousePos;
                ApplyPanDelta(delta);
            }
            else if (!isPressing && _isDragging)
            {
                ResetInputState();
            }
        }

        private void ApplyPanDelta(Vector2 screenDelta)
        {
            if (_config == null || _targetCamera == null) return;

            // Scale pan speed with current zoom factor for consistent feel across heights
            float zoomFactor = _currentZoom / Mathf.Max(1f, _config.DefaultZoom);
            float factor = (_config.PanSpeed * zoomFactor * 0.02f);

            // Invert delta: dragging screen right moves camera left (bringing map right)
            Vector3 panDelta = new Vector3(-screenDelta.x * factor, 0f, -screenDelta.y * factor);

            _targetPosition = CameraMath.ClampPosition(_targetPosition + panDelta, _config.MinBounds, _config.MaxBounds);
        }

        private bool IsPointerOverUI(int pointerId)
        {
            if (EventSystem.current == null) return false;

            if (pointerId >= 0)
            {
                return EventSystem.current.IsPointerOverGameObject(pointerId);
            }

            return EventSystem.current.IsPointerOverGameObject();
        }

        private void ResetInputState()
        {
            _isDragging = false;
            _pointerDownOverUI = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (_config == null) return;

            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(
                (_config.MinBounds.x + _config.MaxBounds.x) * 0.5f,
                0f,
                (_config.MinBounds.y + _config.MaxBounds.y) * 0.5f);

            Vector3 size = new Vector3(
                Mathf.Abs(_config.MaxBounds.x - _config.MinBounds.x),
                1f,
                Mathf.Abs(_config.MaxBounds.y - _config.MinBounds.y));

            Gizmos.DrawWireCube(center, size);
        }
    }
}
