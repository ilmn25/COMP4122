using System.Collections;
using UnityEngine;

namespace Resources.Scripts
{
    public static partial class UI
    {
        public static IEnumerator Scale(bool show, float duration, GameObject target, float scale, float easeSpeed = 0.5f)
        { 
            Vector3 initialScale = target.transform.localScale;
            Vector3 targetScale = Vector3.one * scale;
            float elapsedTime = 0f;

            while (elapsedTime < duration * 0.98f)
            {
                float t = elapsedTime / duration;
                if (show)
                {
                    // if (target.transform.localScale.x > 0.5f) _isInputBlocked = false;
                    t = Mathf.SmoothStep(0f, 1f, Mathf.Pow(t, easeSpeed)); // Apply adjustable ease-out effect
                }
                else
                {
                    t = Mathf.Lerp(0f, 1f, t); // Linear interpolation for hiding
                }

                target.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            target.transform.localScale = targetScale;
        }
    
        public static IEnumerator Slide(bool show, float duration, GameObject target, Vector3 position, float easeSpeed = 0.5f)
        {
            Vector3 initialPos = target.transform.localPosition;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;

                if (show)
                {
                    t = Mathf.SmoothStep(0f, 1f, Mathf.Pow(t, easeSpeed)); // Ease-out
                }
                else
                {
                    t = Mathf.Lerp(0f, 1f, t); // Linear for hiding
                }

                target.transform.localPosition = Vector3.Lerp(initialPos, position, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            target.transform.localPosition = position;
        }
    }
}