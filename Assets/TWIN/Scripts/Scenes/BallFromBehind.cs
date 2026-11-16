using System.Collections;
using UnityEngine;

namespace TWIN.Scenes
{
    /// <summary>
    /// Launch(): the ball appears behind the player, hits them in the back, bounces, then rolls back
    /// to the player's feet (uphill, against physics) and stops.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BallFromBehind : MonoBehaviour
    {
        public float launchDistance = 6f;
        public float launchSpeed = 9f;
        public float bounceSeconds = 1.2f;
        public float rollBackSpeed = 1.5f;
        public float stopDistance = 0.7f;

        Rigidbody _rb;
        Transform _player;
        bool _launched;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
            GetComponent<Renderer>().enabled = false;
        }

        public void Launch()
        {
            if (_launched) return;
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p == null) return;
            _player = p.transform;
            _launched = true;
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            GetComponent<Renderer>().enabled = true;
            Vector3 back = -_player.forward;
            Vector3 start = _player.position + back * launchDistance + Vector3.up * 1.2f;
            transform.position = start;
            _rb.isKinematic = false;
            Vector3 target = _player.position + Vector3.up * 1.0f;
            _rb.linearVelocity = (target - start).normalized * launchSpeed;
            Debug.Log("[TWIN] Ball launched from behind");

            yield return new WaitForSeconds(bounceSeconds);

            // Roll back to the player, uphill, as if pulled.
            _rb.isKinematic = true;
            while (true)
            {
                Vector3 feet = _player.position;
                Vector3 to = feet - transform.position;
                to.y = 0f;
                if (to.magnitude <= stopDistance) break;
                Vector3 step = to.normalized * rollBackSpeed * Time.deltaTime;
                transform.position += step;
                // keep it on the ground
                if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out var hit, 3f))
                    transform.position = new Vector3(transform.position.x, hit.point.y + 0.25f, transform.position.z);
                transform.Rotate(Vector3.Cross(Vector3.up, step), step.magnitude / 0.25f * Mathf.Rad2Deg, Space.World);
                yield return null;
            }
            Debug.Log("[TWIN] Ball rolled back to the player and stopped");
        }
    }
}
