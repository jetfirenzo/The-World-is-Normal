using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TWIN.Core
{
    /// <summary>Full-screen image used for fades. Black by default; endings can fade to white.</summary>
    public class ScreenFader : MonoBehaviour
    {
        public Image image;

        void Awake()
        {
            if (image != null) image.raycastTarget = false;
            ResetToBlack();
        }

        public IEnumerator FadeOut(float seconds) => Fade(0f, 1f, seconds);
        public IEnumerator FadeIn(float seconds) => Fade(1f, 0f, seconds);

        /// <summary>Fade to a colour (white for the Wake Up ending).</summary>
        public IEnumerator FadeOut(float seconds, Color color)
        {
            SetColor(color);
            yield return Fade(0f, 1f, seconds);
        }

        public void SetColor(Color color)
        {
            if (image == null) return;
            var a = image.color.a;
            image.color = new Color(color.r, color.g, color.b, a);
        }

        /// <summary>Transparent and black again (used on restart).</summary>
        public void ResetToBlack()
        {
            if (image == null) return;
            image.color = new Color(0f, 0f, 0f, 0f);
        }

        IEnumerator Fade(float from, float to, float seconds)
        {
            if (image == null) yield break;
            image.gameObject.SetActive(true);
            float t = 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                SetAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(t / seconds)));
                yield return null;
            }
            SetAlpha(to);
        }

        public void SetAlpha(float a)
        {
            if (image == null) return;
            var c = image.color;
            c.a = a;
            image.color = c;
        }
    }
}
