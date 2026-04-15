using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Ilumisoft.MergeDice.Survival
{
    [System.Serializable]
    public class SurvivalResources : MonoBehaviour
    {
        [Header("Mechanics")]
        [Tooltip("When disabled, food and water never decay and can no longer cause game over.")]
        [SerializeField] private bool enableFoodAndWaterMechanics = false;

        [Header("Resource Configuration")]
        [BoxGroup("Food Settings")]
        [Range(1, 1000)]
        [Tooltip("Maximum food capacity")]
        public float maxFood = 100f;
        
        [BoxGroup("Food Settings")]
        [Range(0.1f, 10f)]
        [Tooltip("Food consumption rate per second")]
        public float foodDecayRate = 1f;
        
        [BoxGroup("Food Settings")]
        [Range(1, 50)]
        [Tooltip("Food gained per dice when merging food dice")]
        public float foodPerDice = 5f;
        
        [Space(10)]
        [BoxGroup("Water Settings")]
        [Range(1, 1000)]
        [Tooltip("Maximum water capacity")]
        public float maxWater = 100f;
        
        [BoxGroup("Water Settings")]
        [Range(0.1f, 10f)]
        [Tooltip("Water consumption rate per second")]
        public float waterDecayRate = 1.5f;
        
        [BoxGroup("Water Settings")]
        [Range(1, 50)]
        [Tooltip("Water gained per dice when merging water dice")]
        public float waterPerDice = 7f;
        
        [Header("Current Resources")]
        [BoxGroup("Status")]
        [ProgressBar(0, "maxFood", ColorGetter = "GetFoodColor")]
        [ShowInInspector, ReadOnly]
        public float CurrentFood { get; private set; }
        
        [BoxGroup("Status")]
        [ProgressBar(0, "maxWater", ColorGetter = "GetWaterColor")]
        [ShowInInspector, ReadOnly]
        public float CurrentWater { get; private set; }
        
        [BoxGroup("Status")]
        [ShowInInspector, ReadOnly]
        public bool IsResourcesDepleted => enableFoodAndWaterMechanics && (CurrentFood <= 0 || CurrentWater <= 0);

        public bool AreMechanicsEnabled => enableFoodAndWaterMechanics;
        
        [BoxGroup("Debug")]
        [Button("Add Food")]
        private void DebugAddFood() => AddFood(foodPerDice);
        
        [BoxGroup("Debug")]
        [Button("Add Water")]
        private void DebugAddWater() => AddWater(waterPerDice);
        
        [BoxGroup("Debug")]
        [Button("Reset Resources")]
        private void DebugResetResources() => ResetResources();

        // Events
        public event Action<float, float> OnResourcesChanged; // food, water
        public event Action OnResourcesDepleted;
        public event Action<float> OnFoodAdded;
        public event Action<float> OnWaterAdded;

        private bool isRunning = false;

        #region Odin Color Getters
        private Color GetFoodColor()
        {
            float percentage = CurrentFood / maxFood;
            if (percentage > 0.5f) return Color.green;
            if (percentage > 0.25f) return Color.yellow;
            return Color.red;
        }

        private Color GetWaterColor()
        {
            float percentage = CurrentWater / maxWater;
            if (percentage > 0.5f) return Color.cyan;
            if (percentage > 0.25f) return Color.yellow;
            return Color.red;
        }
        #endregion

        private void Awake()
        {
            ResetResources();
        }

        public void StartResourceDecay()
        {
            isRunning = enableFoodAndWaterMechanics;
        }

        public void StopResourceDecay()
        {
            isRunning = false;
        }

        public void ResetResources()
        {
            CurrentFood = maxFood;
            CurrentWater = maxWater;
            OnResourcesChanged?.Invoke(CurrentFood, CurrentWater);
        }

        private void Update()
        {
            if (!enableFoodAndWaterMechanics || !isRunning) return;

            // Decay resources over time
            CurrentFood -= foodDecayRate * Time.deltaTime;
            CurrentWater -= waterDecayRate * Time.deltaTime;

            // Clamp to minimum of 0
            CurrentFood = Mathf.Max(0, CurrentFood);
            CurrentWater = Mathf.Max(0, CurrentWater);

            // Trigger events
            OnResourcesChanged?.Invoke(CurrentFood, CurrentWater);

            // Check for depletion
            if (IsResourcesDepleted)
            {
                isRunning = false;
                OnResourcesDepleted?.Invoke();
            }
        }

        public void AddFood(float amount)
        {
            if (!enableFoodAndWaterMechanics) return;

            float oldFood = CurrentFood;
            CurrentFood = Mathf.Min(maxFood, CurrentFood + amount);
            float actualAdded = CurrentFood - oldFood;
            
            if (actualAdded > 0)
            {
                OnFoodAdded?.Invoke(actualAdded);
                OnResourcesChanged?.Invoke(CurrentFood, CurrentWater);
                Debug.Log($"Food added: +{actualAdded:F1} (Total: {CurrentFood:F1}/{maxFood})");
            }
        }

        public void AddWater(float amount)
        {
            if (!enableFoodAndWaterMechanics) return;

            float oldWater = CurrentWater;
            CurrentWater = Mathf.Min(maxWater, CurrentWater + amount);
            float actualAdded = CurrentWater - oldWater;
            
            if (actualAdded > 0)
            {
                OnWaterAdded?.Invoke(actualAdded);
                OnResourcesChanged?.Invoke(CurrentFood, CurrentWater);
                Debug.Log($"Water added: +{actualAdded:F1} (Total: {CurrentWater:F1}/{maxWater})");
            }
        }

        /// <summary>
        /// Call this when food dice are merged
        /// </summary>
        public void OnFoodDiceMerged(int diceLevel, int mergeCount = 1)
        {
            if (!enableFoodAndWaterMechanics) return;

            float foodGained = foodPerDice * mergeCount;
            AddFood(foodGained);
        }

        /// <summary>
        /// Call this when water dice are merged
        /// </summary>
        public void OnWaterDiceMerged(int diceLevel, int mergeCount = 1)
        {
            if (!enableFoodAndWaterMechanics) return;

            float waterGained = waterPerDice * mergeCount;
            AddWater(waterGained);
        }

        /// <summary>
        /// Get the food percentage (0-1)
        /// </summary>
        public float GetFoodPercentage() => maxFood > 0 ? CurrentFood / maxFood : 0f;

        /// <summary>
        /// Get the water percentage (0-1)
        /// </summary>
        public float GetWaterPercentage() => maxWater > 0 ? CurrentWater / maxWater : 0f;
    }
}
