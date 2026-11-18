using System;
using UnityEngine;

namespace KingWorldWar.Core.Camera
{
    /// <summary>
    /// Pure, deterministic mathematical calculations for camera panning, zoom, boundaries, and pinch gestures.
    /// </summary>
    public static class CameraMath
    {
        /// <summary>
        /// Clamps a target world position within defined rectangular horizontal (X, Z) boundaries.
        /// Preserves Y coordinate unchanged.
        /// </summary>
        public static Vector3 ClampPosition(Vector3 position, Vector2 minXZ, Vector2 maxXZ)
        {
            float clampedX = Mathf.Clamp(position.x, Mathf.Min(minXZ.x, maxXZ.x), Mathf.Max(minXZ.x, maxXZ.x));
            float clampedZ = Mathf.Clamp(position.z, Mathf.Min(minXZ.y, maxXZ.y), Mathf.Max(minXZ.y, maxXZ.y));
            return new Vector3(clampedX, position.y, clampedZ);
        }

        /// <summary>
        /// Clamps a zoom level or camera distance between specified minimum and maximum values.
        /// </summary>
        public static float ClampZoom(float zoom, float minZoom, float maxZoom)
        {
            float lower = Mathf.Min(minZoom, maxZoom);
            float upper = Mathf.Max(minZoom, maxZoom);
            return Mathf.Clamp(zoom, lower, upper);
        }

        /// <summary>
        /// Computes the ratio of distance between two touch points to determine pinch zoom scaling.
        /// Returns a value > 1 for zooming in (spread), < 1 for zooming out (pinch), or 1 if invalid.
        /// </summary>
        public static float CalculatePinchRatio(Vector2 touch0Prev, Vector2 touch0Curr, Vector2 touch1Prev, Vector2 touch1Curr)
        {
            float prevDist = Vector2.Distance(touch0Prev, touch1Prev);
            float currDist = Vector2.Distance(touch0Curr, touch1Curr);

            if (prevDist <= 0.0001f || currDist <= 0.0001f)
            {
                return 1.0f;
            }

            return currDist / prevDist;
        }

        /// <summary>
        /// Calculates the smooth damped velocity and target position step using a critically damped spring.
        /// </summary>
        public static Vector3 SmoothDampPosition(
            Vector3 current,
            Vector3 target,
            ref Vector3 currentVelocity,
            float smoothTime,
            float deltaTime,
            float maxSpeed = Mathf.Infinity)
        {
            if (deltaTime <= 0f) return current;
            return Vector3.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

        /// <summary>
        /// Calculates smooth damping for a scalar zoom distance.
        /// </summary>
        public static float SmoothDampZoom(
            float current,
            float target,
            ref float currentVelocity,
            float smoothTime,
            float deltaTime)
        {
            if (deltaTime <= 0f) return current;
            return Mathf.SmoothDamp(current, target, ref currentVelocity, smoothTime, Mathf.Infinity, deltaTime);
        }
    }
}
