using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ilumisoft.MergeDice
{
    public class DiceLevelBehaviour : LevelUpBehaviour
    {
        int currentLevel = 0;
        bool levelSetByFactory = false; // Track if level was explicitly set

        public override int MaxLevel => DiceLevelManager.Instance != null ? DiceLevelManager.Instance.MaxLevel : 5;

        public override UnityAction OnLevelChanged { get; set; }

        private void Start()
        {
            // Only set random level if the level hasn't been explicitly set by a factory
            if (!levelSetByFactory)
            {
                // Check if we're in survival mode for better tile distribution
                var survivalMode = FindFirstObjectByType<Survival.SurvivalGameMode>();
                if (survivalMode != null)
                {
                    // Use survival-friendly random generation
                    CurrentLevel = GetSurvivalRandomLevel();
                }
                else
                {
                    // Use standard random generation
                    CurrentLevel = Random.Range(0, GameStats.MaxReachedLevel + 1);
                }
            }
            else
            {
                // Level was already set by a factory, don't override it
                // Debug.Log($"DiceLevelBehaviour: Level explicitly set to {currentLevel} by factory, not overriding");
            }
        }

        /// <summary>
        /// Returns a random level optimized for survival mode gameplay
        /// </summary>
        private int GetSurvivalRandomLevel()
        {
            // 70% chance for levels 0-1 (easy to combine)
            if (Random.value < 0.7f)
            {
                return Random.Range(0, 2);
            }
            // 30% chance for higher levels but respect the actual MaxLevel
            else
            {
                return Random.Range(2, MaxLevel + 1);
            }
        }

        public override int CurrentLevel
        {
            get => currentLevel;

            set
            {
                currentLevel = Mathf.Clamp(value, 0, MaxLevel);
                levelSetByFactory = true; // Mark that level was explicitly set
                // Debug.Log($"DiceLevelBehaviour: Level set to {currentLevel} (clamped from {value})");

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
                return DiceLevelManager.Instance != null 
                    ? DiceLevelManager.Instance.GetLevelColor(currentLevel) 
                    : Color.white;
            }
        }

        public Sprite Overlay
        {
            get
            {
                return DiceLevelManager.Instance != null 
                    ? DiceLevelManager.Instance.GetLevelOverlay(currentLevel) 
                    : null;
            }
        }
    }
}