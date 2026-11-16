using UnityEngine;
using UnityEngine.Events;

namespace TWIN.Player
{
    /// <summary>Fires when the player (tag "Player") walks into this trigger collider.</summary>
    [RequireComponent(typeof(Collider))]
    public class TriggerZone : MonoBehaviour
    {
        public bool oneShot = true;
        public UnityEvent onPlayerEnter = new UnityEvent();
        public bool Fired { get; private set; }

        void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (oneShot && Fired) return;
            Fired = true;
            onPlayerEnter.Invoke();
        }
    }
}
