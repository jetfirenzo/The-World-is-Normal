using System;
using UnityEngine;
using UnityEngine.Events;

namespace TWIN.Player
{
    /// <summary>
    /// Anything the player can "use": the alarm, a note, a door, the shopkeeper.
    /// Desktop: looked at + click/E. VR (later): an XRI interactor can call Interact() from its Select event.
    /// Needs a Collider on this object or a child.
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        [Tooltip("Shown at the crosshair while looking at this")]
        public string prompt = "Use";
        [Tooltip("Can it be used right now? Scene scripts toggle this to gate progress.")]
        public bool available = true;
        [Tooltip("Becomes unavailable after the first use")]
        public bool oneShot = true;

        public UnityEvent onInteract = new UnityEvent();
        public event Action<Interactable> Interacted;

        public bool IsAvailable => available && isActiveAndEnabled;
        public bool WasUsed { get; private set; }

        public void Interact()
        {
            if (!IsAvailable) return;
            WasUsed = true;
            if (oneShot) available = false;
            Debug.Log($"[TWIN] Interact: {name}");
            onInteract.Invoke();
            Interacted?.Invoke(this);
        }

        public void SetAvailable(bool value) => available = value;
    }
}
