using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Ilumisoft.MergeDice
{
    [CreateAssetMenu(fileName = "DiceLevelManager", menuName = "Merge Dice/Dice Level Manager", order = 0)]
    public class DiceLevelManager : ScriptableObject
    {
        [Title("Dice Level Configuration")]
        [SerializeField, TableList(ShowIndexLabels = true, DrawScrollView = true, MaxScrollViewHeight = 400)]
        [Tooltip("Configure all dice levels with their visual properties")]
        private List<DiceLevel> diceLevels = new List<DiceLevel>();

        [Title("Runtime Information")]
        [ShowInInspector, ReadOnly]
        [PropertyTooltip("Maximum level index (Count - 1)")]
        public int MaxLevel => diceLevels.Count - 1;

        [ShowInInspector, ReadOnly]
        [PropertyTooltip("Total number of dice levels configured")]
        public int TotalLevels => diceLevels.Count;

        private static DiceLevelManager instance;
        public static DiceLevelManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<DiceLevelManager>("DiceLevelManager");
                    if (instance == null)
                    {
                        Debug.LogError("DiceLevelManager not found in Resources folder! Please create one using the menu: Assets > Create > Merge Dice > Dice Level Manager");
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Get dice level data by index
        /// </summary>
        public DiceLevel GetDiceLevel(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= diceLevels.Count)
            {
                Debug.LogWarning($"Invalid dice level index: {levelIndex}. Using level 0 as fallback.");
                return diceLevels.Count > 0 ? diceLevels[0] : default(DiceLevel);
            }
            return diceLevels[levelIndex];
        }

        /// <summary>
        /// Get sprite for a specific dice level
        /// </summary>
        public Sprite GetLevelSprite(int levelIndex)
        {
            return GetDiceLevel(levelIndex).sprite;
        }

        /// <summary>
        /// Get sprite overlay for a specific dice level
        /// </summary>
        public Sprite GetLevelOverlay(int levelIndex)
        {
            return GetDiceLevel(levelIndex).overlay;
        }

        /// <summary>
        /// Check if a level index is valid
        /// </summary>
        public bool IsValidLevel(int levelIndex)
        {
            return levelIndex >= 0 && levelIndex < diceLevels.Count;
        }

        /// <summary>
        /// Get all available dice levels (for editor purposes)
        /// </summary>
        public List<DiceLevel> GetAllLevels()
        {
            return new List<DiceLevel>(diceLevels);
        }

        [Button(ButtonSizes.Medium)]
        [GUIColor(0.8f, 1f, 0.8f)]
        [PropertyTooltip("Add a new dice level configuration")]
        private void AddNewLevel()
        {
            diceLevels.Add(new DiceLevel
            {
                    sprite = null,
                overlay = null
            });
        }

        [Button(ButtonSizes.Medium)]
        [GUIColor(1f, 0.8f, 0.8f)]
        [PropertyTooltip("Remove the last dice level")]
        [ShowIf("@diceLevels.Count > 0")]
        private void RemoveLastLevel()
        {
            if (diceLevels.Count > 0)
            {
                diceLevels.RemoveAt(diceLevels.Count - 1);
            }
        }

        [Button(ButtonSizes.Large)]
        [GUIColor(0.8f, 0.8f, 1f)]
        [PropertyTooltip("Create default dice level setup (6 levels)")]
        [ShowIf("@diceLevels.Count == 0")]
        private void CreateDefaultLevels()
        {
            diceLevels.Clear();
            diceLevels.AddRange(new DiceLevel[]
            {
                    new DiceLevel { sprite = null, overlay = null },
                    new DiceLevel { sprite = null, overlay = null },
                    new DiceLevel { sprite = null, overlay = null },
                    new DiceLevel { sprite = null, overlay = null },
                    new DiceLevel { sprite = null, overlay = null },
                    new DiceLevel { sprite = null, overlay = null }
            });
        }

        private void OnValidate()
        {
            // Ensure we always have at least one level
            if (diceLevels.Count == 0)
            {
                    diceLevels.Add(new DiceLevel { sprite = null, overlay = null });
            }
        }
    }
}
