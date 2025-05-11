using UnityEngine;
using UnityEngine.Events;

namespace Ilumisoft.MergeDice
{
    public class DiceGameTile : GameTile, ICanLevelUp, ICanMoveTo
    {
        [SerializeField]
        LevelUpBehaviour levelUpBehaviour = null;

        [SerializeField]
        MoveToBehaviour moveToBehaviour = null;

        public int CurrentLevel
        {
            get => levelUpBehaviour.CurrentLevel;
            set => levelUpBehaviour.CurrentLevel = value;
        }

        public int MaxLevel => levelUpBehaviour.MaxLevel;

        public UnityAction OnLevelChanged
        {
            get => levelUpBehaviour.OnLevelChanged;
            set => levelUpBehaviour.OnLevelChanged = value;
        }

        public bool IsMoving => moveToBehaviour.IsMoving;

        public bool HasMaxLevel => levelUpBehaviour.HasMaxLevel;

        public void LevelUp() => levelUpBehaviour.LevelUp();

        public void MoveTo(Vector3 target)
        {
            moveToBehaviour.MoveTo(target);
        }

        public void MoveTo(Vector3 target, float duration)
        {
            moveToBehaviour.MoveTo(target, duration);
        }

        void Reset()
        {
            levelUpBehaviour = GetComponentInChildren<LevelUpBehaviour>();
            moveToBehaviour = GetComponentInChildren<MoveToBehaviour>();
        }
    }
}