using TMPro;
using TWIN.Core;
using TWIN.Player;
using UnityEngine;
using UnityEngine.UI;

namespace TWIN.Scenes
{
    /// <summary>Placeholder end screen: final state plus the ending that WOULD play.</summary>
    public class EndOfDemoScreen : MonoBehaviour
    {
        public TMP_Text text;
        public Button restartButton;

        void Start()
        {
            var gm = GameManager.Instance;
            var s = gm.State;
            if (text != null)
            {
                text.text =
                    $"<size=160%>{gm.lines.endOfDemo}</size>\n\n" +
                    $"strangeCount: {s.strangeCount}\n" +
                    $"route: {s.route}\n" +
                    $"notePocketed: {s.notePocketed}\n\n" +
                    $"ending that would play:\n<b>{EndingResolver.DisplayName(EndingResolver.Resolve(s))}</b>\n\n" +
                    "<size=60%>press R or the button to start a new run</size>";
            }
            if (restartButton != null) restartButton.onClick.AddListener(() => { DesktopRig.SetCursorFree(false); gm.RestartDemo(); });
            DesktopRig.SetCursorFree(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void OnDestroy()
        {
            DesktopRig.SetCursorFree(false);
        }
    }
}
