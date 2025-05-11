using UnityEngine;
using UnityEngine.Events;

namespace Ilumisoft.MergeDice
{
    public abstract class LevelUpBehaviour : MonoBehaviour, ICanLevelUp
    {
        public abstract int CurrentLevel { get; set; }

        public abstract int MaxLevel { get; }

        public abstract UnityAction OnLevelChanged { get; set; }

        public virtual bool HasMaxLevel => CurrentLevel == MaxLevel;

        public abstract void LevelUp();
    }
}