using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace Ilumisoft.MergeDice
{
    public class OverlayCanvas : MonoBehaviour
    {
        [SerializeField]
        PlayableDirector playableDirector = null;

        [SerializeField]
        PlayableAsset fadeInTimeline = null;

        [SerializeField]
        PlayableAsset fadeOutTimeline = null;

        /// <summary>
        /// This can be used in a coroutine to smoothly fade the overlay in
        /// </summary>
        /// <returns></returns>
        public IEnumerator FadeIn()
        {
            yield return Play(fadeInTimeline);
        }

        /// <summary>
        /// This can be used in a coroutine to smoothly fade the overlay out
        /// </summary>
        /// <returns></returns>
        public IEnumerator FadeOut()
        {
            yield return Play(fadeOutTimeline);
        }

        private IEnumerator Play(PlayableAsset playableAsset)
        {
            playableDirector.playableAsset = playableAsset;
            playableDirector.Play();

            yield return new WaitForSecondsRealtime((float)playableAsset.duration);
        }
    }
}