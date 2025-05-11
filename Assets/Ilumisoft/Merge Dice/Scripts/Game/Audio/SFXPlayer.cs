using Ilumisoft.MergeDice.Events;
using UnityEngine;

namespace Ilumisoft.MergeDice
{
    public class SFXPlayer : MonoBehaviour
    {
        [SerializeField]
        AudioSource audioSource = null;

        [SerializeField]
        SFXDatabase database = new SFXDatabase();

        private void Reset()
        {
            audioSource = GetComponentInChildren<AudioSource>();
        }

        private void OnEnable()
        {
            GameEvents<SFXEventType>.OnTrigger += OnTriggerSFX;
        }

        private void OnDisable()
        {
            GameEvents<SFXEventType>.OnTrigger -= OnTriggerSFX;
        }

        public void OnTriggerSFX(SFXEventType gameSound)
        {
            if (database != null && database.TryGetSFX(gameSound, out var audioClip))
            {
                PlayOneShot(audioClip);
            }
        }

        void PlayOneShot(AudioClip clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}