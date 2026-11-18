using UnityEngine;

namespace KingWorldWar.Features.Camera
{
    /// <summary>
    /// Configuration asset for StrategyCameraRig pan, zoom, boundaries, and damping.
    /// </summary>
    [CreateAssetMenu(fileName = "CameraConfig_Default", menuName = "KingWorldWar/Camera/Camera Config")]
    public class CameraConfigSO : ScriptableObject
    {
        [Header("Movement & Damping")]
        [Tooltip("Pan speed multiplier for drag movement.")]
        [SerializeField] private float _panSpeed = 1.0f;

        [Tooltip("Smooth time for position damping in seconds.")]
        [SerializeField] private float _positionSmoothTime = 0.12f;

        [Header("Zoom Settings")]
        [Tooltip("Minimum camera distance / height.")]
        [SerializeField] private float _minZoom = 10.0f;

        [Tooltip("Maximum camera distance / height.")]
        [SerializeField] private float _maxZoom = 70.0f;

        [Tooltip("Default zoom height on initialization.")]
        [SerializeField] private float _defaultZoom = 35.0f;

        [Tooltip("Smooth time for zoom damping in seconds.")]
        [SerializeField] private float _zoomSmoothTime = 0.10f;

        [Tooltip("Mouse scroll wheel zoom sensitivity.")]
        [SerializeField] private float _mouseScrollSensitivity = 5.0f;

        [Tooltip("Touch pinch zoom sensitivity.")]
        [SerializeField] private float _pinchSensitivity = 20.0f;

        [Header("Orientation")]
        [Tooltip("Fixed camera pitch angle in degrees (e.g. 55 for RTS angle).")]
        [Range(20f, 85f)]
        [SerializeField] private float _pitchAngle = 55.0f;

        [Header("World Boundaries (Ground XZ)")]
        [Tooltip("Minimum (X, Z) ground coordinates.")]
        [SerializeField] private Vector2 _minBounds = new Vector2(-60f, -60f);

        [Tooltip("Maximum (X, Z) ground coordinates.")]
        [SerializeField] private Vector2 _maxBounds = new Vector2(60f, 60f);

        public float PanSpeed => _panSpeed;
        public float PositionSmoothTime => _positionSmoothTime;
        public float MinZoom => _minZoom;
        public float MaxZoom => _maxZoom;
        public float DefaultZoom => _defaultZoom;
        public float ZoomSmoothTime => _zoomSmoothTime;
        public float MouseScrollSensitivity => _mouseScrollSensitivity;
        public float PinchSensitivity => _pinchSensitivity;
        public float PitchAngle => _pitchAngle;
        public Vector2 MinBounds => _minBounds;
        public Vector2 MaxBounds => _maxBounds;

        public static CameraConfigSO CreateDefaultInstance()
        {
            var config = CreateInstance<CameraConfigSO>();
            return config;
        }
    }
}
