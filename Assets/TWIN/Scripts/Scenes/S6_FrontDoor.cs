using TMPro;
using TWIN.Core;
using TWIN.Player;
using UnityEngine;

namespace TWIN.Scenes
{
    /// <summary>Scene 6 (normal): the shop's front door opened straight into the bedroom. The clock says 11:11.</summary>
    public class S6_FrontDoor : MonoBehaviour
    {
        public TMP_Text clockText;
        public Interactable bed;

        void Start()
        {
            if (clockText != null) clockText.text = "11:11";
            if (bed != null)
            {
                bed.prompt = "Go to sleep";
                bed.onInteract.AddListener(() => GameManager.Instance.GoToNextScene());
            }
        }
    }
}
