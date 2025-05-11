using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ilumisoft.MergeDice
{
    public class DiceLevelBehaviour : LevelUpBehaviour
    {
        int currentLevel = 0;

        [SerializeField]
        List<DiceLevel> diceLevels = new List<DiceLevel>();

        public override int MaxLevel => diceLevels.Count - 1;

        public override UnityAction OnLevelChanged { get; set; }

        private void Start()
        {
            CurrentLevel = Random.Range(0, GameStats.MaxReachedLevel + 1);
        }

        public override int CurrentLevel
        {
            get => currentLevel;

            set
            {
                currentLevel = Mathf.Clamp(value, 0, MaxLevel);

                OnLevelChanged?.Invoke();
            }
        }

        public override void LevelUp()
        {
            if (currentLevel < MaxLevel)
            {
                CurrentLevel++;
            }
        }

        public Color Color
        {
            get
            {
                return diceLevels[currentLevel].Color;
            }
        }

        public Sprite Overlay
        {
            get
            {
                return diceLevels[currentLevel].overlay;
            }
        }
    }
}