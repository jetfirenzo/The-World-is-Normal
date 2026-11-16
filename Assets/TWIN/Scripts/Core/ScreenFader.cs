using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TWIN.Core
{
    /// <summary>Full-screen black image used for scene transitions. Starts transparent.</summary>
    public class ScreenFader : MonoBehaviour
    {
        public Image image;

        void Awake()
        {
            if (image != null) image.raycastTarget = false;
            SetAlpha(0f);
        }

        public IEnumerator FadeOut(float seconds) => Fade(0f, 1f, seconds);
        public IEnumerator FadeIn(float seconds) => Fade(1f, 0f, seconds);

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
