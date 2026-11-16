using TMPro;
using TWIN.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TWIN.Debugging
{
    /// <summary>
    /// F1 toggles a state readout. F5..F12 jump to a scene, N skips to the next scene, R restarts the demo.
    /// </summary>
    public class DebugOverlay : MonoBehaviour
    {
        public TMP_Text text;
        public bool visibleAtStart = true;

        void Start()
        {
            if (text != null) text.gameObject.SetActive(visibleAtStart);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            var kb = Keyboard.current;
            if (kb != null && gm != null)
            {
                if (kb.f1Key.wasPressedThisFrame && text != null) text.gameObject.SetActive(!text.gameObject.activeSelf);
                if (kb.nKey.wasPressedThisFrame) gm.GoToNextScene();
                if (kb.rKey.wasPressedThisFrame) gm.RestartDemo();
                var f = gm.flow;
                if (kb.f5Key.wasPressedThisFrame) gm.LoadScene(f.bedroom);
                if (kb.f6Key.wasPressedThisFrame) gm.LoadScene(f.bathroom);
                if (kb.f7Key.wasPressedThisFrame) gm.LoadScene(f.livingRoom);
                if (kb.f8Key.wasPressedThisFrame) gm.LoadScene(f.street);
                if (kb.f9Key.wasPressedThisFrame) gm.LoadScene(f.shop);
                if (kb.f10Key.wasPressedThisFrame) gm.LoadScene(f.frontDoor);
                if (kb.f11Key.wasPressedThisFrame) gm.LoadScene(f.backrooms);
                if (kb.f12Key.wasPressedThisFrame) gm.LoadScene(f.lastMorning);
            }

            if (text == null || !text.gameObject.activeSelf || gm == null) return;
            var s = gm.State;
            text.text =
                $"<b>TWIN debug</b> (F1 hide)\n" +
                $"scene: {gm.CurrentScene}\n" +
                $"strangeCount: {s.strangeCount}   route: {s.route}   notePocketed: {s.notePocketed}\n" +
                $"forks: {Fork(s, 0)} {Fork(s, 1)} {Fork(s, 2)} {Fork(s, 3)}\n" +
                $"ending: {EndingResolver.Resolve(s)}\n" +
                $"<size=70%>N next scene | R restart | F5-F12 jump | 1/2 vote | Esc cursor</size>";
        }

        static string Fork(GameState s, int i) => !s.forkAnswered[i] ? "-" : (s.forkWasStrange[i] ? "S" : "n");
    }
}
