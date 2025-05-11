using Ilumisoft.MergeDice.Events;
using System.Collections.Generic;
using UnityEngine;

namespace Ilumisoft.MergeDice
{
    [System.Serializable]
    public class SFXDatabase : ISerializationCallbackReceiver
    {
        [SerializeField]
        List<SFXDefinition> soundEffects = new List<SFXDefinition>();

        Dictionary<SFXEventType, AudioClip> sfxDictionary = new Dictionary<SFXEventType, AudioClip>();

        public bool TryGetSFX(SFXEventType gameSound, out AudioClip audioClip)
        {
            return sfxDictionary.TryGetValue(gameSound, out audioClip);
        }

        public void OnAfterDeserialize()
        {
            sfxDictionary.Clear();

            foreach (var sfx in soundEffects)
            {
                if (sfxDictionary.ContainsKey(sfx.GameSound) == false)
                {
                    sfxDictionary.Add(sfx.GameSound, sfx.AudioClip);
                }
            }
        }

        public void OnBeforeSerialize() { }
    }
}