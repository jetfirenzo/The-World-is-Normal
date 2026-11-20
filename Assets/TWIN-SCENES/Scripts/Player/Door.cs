using TWIN.Core;
using UnityEngine;

namespace TWIN.Player
{
    /// <summary>
    /// A door the player uses to leave the scene. Locked until the scene script calls Unlock().
    /// Loads the next scene from the SceneFlow, or a specific scene when targetScene is set.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class Door : MonoBehaviour
    {
        public string lockedPrompt = "It won't open yet";
        public string openPrompt = "Open the door";
        [Tooltip("Leave empty to follow the SceneFlow")]
        public string targetScene = "";
        public bool startLocked = true;

        Interactable _interactable;
        bool _locked;

        void Awake()
        {
            _interactable = GetComponent<Interactable>();
            _interactable.oneShot = false;
            _interactable.onInteract.AddListener(OnUse);
            SetLocked(startLocked);
        }

        public void Unlock() => SetLocked(false);
        public void Lock() => SetLocked(true);

        public void SetLocked(bool locked)
        {
            _locked = locked;
            _interactable.prompt = locked ? lockedPrompt : openPrompt;
        }

        void OnUse()
        {
            if (_locked) return;
            _interactable.available = false;
            var gm = GameManager.Instance;
            if (gm == null) return;
            if (string.IsNullOrEmpty(targetScene)) gm.GoToNextScene();
            else gm.LoadScene(targetScene);
        }
    }
}
