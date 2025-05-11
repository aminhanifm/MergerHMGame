using Ilumisoft.MergeDice.Events;
using UnityEngine;

namespace Ilumisoft.MergeDice
{
#pragma warning disable 0649
    [System.Serializable]
    public struct SFXDefinition
    {
        [SerializeField]
        private SFXEventType gameSound;

        [SerializeField]
        private AudioClip audioClip;

        public SFXEventType GameSound => gameSound;

        public AudioClip AudioClip => audioClip;
    }
#pragma warning restore 0649
}