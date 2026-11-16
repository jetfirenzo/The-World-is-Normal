using UnityEngine;

namespace TWIN.Player
{
    /// <summary>Helpers for "is the player looking at X" (used by the child in the street).</summary>
    public static class LookAwareness
    {
        /// <summary>True if any part of the renderer bounds is inside the camera frustum.</summary>
        public static bool IsInView(Camera cam, Renderer r)
        {
            if (cam == null || r == null) return false;
            var planes = GeometryUtility.CalculateFrustumPlanes(cam);
            return GeometryUtility.TestPlanesAABB(planes, r.bounds);
        }

        /// <summary>True if the point is in front of the camera within the given half angle.</summary>
        public static bool IsFacing(Transform viewer, Vector3 point, float halfAngleDegrees)
        {
            var to = point - viewer.position;
            to.y = 0f;
            if (to.sqrMagnitude < 0.0001f) return true;
            var fwd = viewer.forward; fwd.y = 0f;
            return Vector3.Angle(fwd, to) <= halfAngleDegrees;
        }
    }
}
