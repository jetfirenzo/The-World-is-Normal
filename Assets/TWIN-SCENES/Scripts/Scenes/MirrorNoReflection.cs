using UnityEngine;

namespace TWIN.Scenes
{
    /// <summary>
    /// A planar mirror that reflects the room but not the player: the reflection camera culls the "Player" layer.
    /// Put this on the mirror quad (its forward = mirror normal, pointing INTO the room). Desktop/mono only.
    /// </summary>
    public class MirrorNoReflection : MonoBehaviour
    {
        public Camera reflectionCamera;
        public Renderer mirrorRenderer;
        [Tooltip("Layers that must NOT appear in the mirror (the player)")]
        public LayerMask hiddenLayers;
        public int textureSize = 1024;
        [Tooltip("Flip if the mirror shows the wall behind it instead of the room")]
        public bool flipClipPlane;

        RenderTexture _rt;

        /// <summary>A Quad's visible side is its local -Z, so the mirror normal (pointing into the room) is -forward.</summary>
        public Vector3 Normal => -transform.forward;

        void Start()
        {
            _rt = new RenderTexture(textureSize, textureSize, 24) { name = "TWIN_MirrorRT" };
            if (reflectionCamera != null)
            {
                reflectionCamera.targetTexture = _rt;
                reflectionCamera.cullingMask &= ~hiddenLayers.value;
            }
            if (mirrorRenderer != null)
            {
                var m = mirrorRenderer.material;
                m.mainTexture = _rt;
                m.mainTextureScale = new Vector2(-1f, 1f); // a real mirror flips left/right
                m.mainTextureOffset = new Vector2(1f, 0f);
            }
        }

        void LateUpdate()
        {
            var main = Camera.main;
            if (main == null || reflectionCamera == null) return;

            Vector3 n = Normal;
            Vector3 p = transform.position;
            float d = -Vector3.Dot(n, p);

            // Mirror the main camera's position and orientation across the plane.
            Vector3 camPos = main.transform.position;
            Vector3 reflPos = camPos - 2f * (Vector3.Dot(camPos, n) + d) * n;
            Vector3 fwd = Vector3.Reflect(main.transform.forward, n);
            Vector3 up = Vector3.Reflect(main.transform.up, n);

            reflectionCamera.transform.SetPositionAndRotation(reflPos, Quaternion.LookRotation(fwd, up));
            reflectionCamera.fieldOfView = main.fieldOfView;
            reflectionCamera.nearClipPlane = main.nearClipPlane;
            reflectionCamera.farClipPlane = main.farClipPlane;

            // Oblique near plane so nothing behind the mirror (the wall) is drawn.
            reflectionCamera.ResetProjectionMatrix();
            Vector3 cpos = reflectionCamera.worldToCameraMatrix.MultiplyPoint(p);
            Vector3 cnormal = reflectionCamera.worldToCameraMatrix.MultiplyVector(n).normalized;
            float sign = flipClipPlane ? -1f : 1f;
            Vector4 clip = new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal)) * sign;
            reflectionCamera.projectionMatrix = reflectionCamera.CalculateObliqueMatrix(clip);
        }

        void OnDestroy()
        {
            if (_rt != null) _rt.Release();
        }
    }
}
