using TWIN.Player;
using UnityEngine;

namespace TWIN.Scenes
{
    /// <summary>
    /// Stands still while the player can see it; moves closer whenever the player is not looking.
    /// Also used as the shopkeeper's "face" (same prefab/material) in scene 5 with moving disabled.
    /// </summary>
    public class WeepingChild : MonoBehaviour
    {
        public Renderer bodyRenderer;
        public float speed = 1.2f;
        public float stopDistance = 1.5f;
        public bool canMove = true;
        public LayerMask occluders = ~0;

        Transform _player;

        void Start()
        {
            if (bodyRenderer == null) bodyRenderer = GetComponentInChildren<Renderer>();
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;
        }

        void Update()
        {
            if (_player == null) return;
            var cam = Camera.main;

            // Always face the player.
            Vector3 to = _player.position - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(to);

            if (!canMove || cam == null) return;
            if (IsSeen(cam)) return;
            if (to.magnitude <= stopDistance) return;
            transform.position += to.normalized * speed * Time.deltaTime;
            SnapToGround();
        }

        void SnapToGround()
        {
            var origin = transform.position + Vector3.up * 2f;
            int mask = occluders & ~(1 << gameObject.layer);
            var hits = Physics.RaycastAll(origin, Vector3.down, 6f, mask, QueryTriggerInteraction.Ignore);
            float best = float.NegativeInfinity;
            foreach (var h in hits)
            {
                if (h.collider.transform.IsChildOf(transform)) continue;
                if (h.point.y > best) best = h.point.y;
            }
            if (best > float.NegativeInfinity)
                transform.position = new Vector3(transform.position.x, best, transform.position.z);
        }

        bool IsSeen(Camera cam)
        {
            if (!LookAwareness.IsInView(cam, bodyRenderer)) return false;
            // Occlusion: if something solid is between the camera and the child, the child is not seen.
            Vector3 eye = cam.transform.position;
            Vector3 head = bodyRenderer.bounds.center + Vector3.up * bodyRenderer.bounds.extents.y * 0.8f;
            if (Physics.Linecast(eye, head, out var hit, occluders, QueryTriggerInteraction.Ignore))
                return hit.collider.transform.IsChildOf(transform);
            return true;
        }
    }
}
