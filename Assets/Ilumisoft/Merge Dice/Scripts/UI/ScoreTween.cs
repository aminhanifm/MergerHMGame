using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Ilumisoft.MergeDice
{
    public class ScoreTween
    {
        Text text;

        Coroutine runningRoutine;

        public ScoreTween(Text text)
        {
            this.text = text;
        }

        public void Fade(int from, int to, float duration)
        {
            if (runningRoutine != null)
            {
                text.StopCoroutine(runningRoutine);
            }

            runningRoutine = text.StartCoroutine(FadeCoroutine(from, to, duration));
        }

        IEnumerator FadeCoroutine(int from, int to, float duration)
        {
            float timeElapsed = 0.0f;

            while (timeElapsed < duration)
            {
                timeElapsed += Time.deltaTime;

                int value = (int)Mathf.Lerp(from, to, timeElapsed / duration);

                text.text = value.ToString();

                yield return null;
            }
        }

    }
}