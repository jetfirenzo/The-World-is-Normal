using TMPro;
using TWIN.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TWIN.Scenes
{
    /// <summary>Title screen. Any key or click starts scene 1.</summary>
    public class S0_Boot : MonoBehaviour
    {
        public TMP_Text title;
        bool _started;

        void Start()
        {
            if (title != null) title.text = "THE WORLD IS NORMAL\n<size=40%>greybox demo\n\npress any key</size>";
        }

        void Update()
        {
            if (_started) return;
            bool any = (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                       || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
            if (!any) return;
            _started = true;
            GameManager.Instance.State.Reset();
            GameManager.Instance.GoToNextScene();
        }
    }
}
