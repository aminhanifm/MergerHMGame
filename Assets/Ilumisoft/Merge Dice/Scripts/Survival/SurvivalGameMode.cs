using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Ilumisoft.MergeDice;
using Ilumisoft.MergeDice.Operations;
using Ilumisoft.MergeDice.Events;
using Ilumisoft.MergeDice.Notifications;
using Sirenix.OdinInspector;

namespace Ilumisoft.MergeDice.Survival
{
    /// <summary>
    /// Different modes for distributing dice levels when spawning tiles
    /// </summary>
    public enum TileDistributionMode
    {
        [Tooltip("Equal chance for all levels")]
        EvenDistribution,
        
        [Tooltip("Favor lower levels with bias control")]
        LowLevelBias,
        
        [Tooltip("Use animation curve for custom distribution")]
        WeightedCurve,
        
        [Tooltip("Only spawn the lowest levels (0-1)")]
        OnlyLowLevels,
        
        [Tooltip("Random distribution with slight preference for mid-levels")]
        BalancedRandom
    }

    public class SurvivalGameMode : GameMode
    {
        [BoxGroup("Core References")]
        [SerializeField] GameBoard gameBoard = null;
        [BoxGroup("Core References")]
        [SerializeField] SelectionLineRenderer lineRenderer = null;
        [BoxGroup("Core References")]
        [SerializeField] SurvivalTimer survivalTimer = null;
        [BoxGroup("Core References")]
        [SerializeField] QuestSystem questSystem = null;
        [BoxGroup("Core References")]
        [SerializeField] SurvivalResources survivalResources = null;

        public SurvivalResources SurvivalResources => survivalResources;

        [BoxGroup("Dice Type Configuration")]
        [Tooltip("Dice level that represents food dice")]
        [Range(0, 10)]
        public int foodDiceLevel = 7; // Assuming level 7 is food dice
        
        [BoxGroup("Dice Type Configuration")]
        [Tooltip("Dice level that represents water dice")]
        [Range(0, 10)]
        public int waterDiceLevel = 8; // Assuming level 8 is water dice

        [BoxGroup("Game State")]
        ISelection selection;
        [BoxGroup("Game State")]
        OperationQueue operations = new OperationQueue();
        [BoxGroup("Game State")]
        bool questCompleted = false;
        [BoxGroup("Game State")]
        bool timesUp = false;
        [BoxGroup("Game State")]
        bool waitingForNextDay = false; // Add flag for waiting state
        [BoxGroup("Game State")]
        bool resourcesDepleted = false; // New flag for resource depletion
        [BoxGroup("Game State")]
        GameTileTracker tileTracker; // Add tracker reference
        ISpawner gameBoardSpawner;
        IGameOverCheck gameOverCheck;

        [BoxGroup("Survival Settings")]
        [Tooltip("Allow unlimited board resets when no moves are available")]
        public bool unlimitedMode = false; // allow unlimited resets

        // Survival mode settings
        [BoxGroup("Tile Generation")]
        [SerializeField] bool destructiveMerge = true; // Destroy tiles instead of leveling up
        [BoxGroup("Tile Generation")]
        [SerializeField] int maxTileLevel = 6; // Limit tile levels for better gameplay (now matches your dice max)
        
        [BoxGroup("Tile Generation")]
        [Tooltip("Choose how dice levels are distributed when spawning")]
        public TileDistributionMode distributionMode = TileDistributionMode.LowLevelBias;
        
        [BoxGroup("Tile Generation")]
        [ShowIf("distributionMode", TileDistributionMode.LowLevelBias)]
        [Range(0f, 1f)]
        [Tooltip("Probability to spawn lower levels (0-1 range)")]
        [SerializeField] float lowLevelBias = 0.7f;
        
        [BoxGroup("Tile Generation")]
        [ShowIf("distributionMode", TileDistributionMode.WeightedCurve)]
        [Tooltip("Animation curve defining probability distribution across levels")]
        [SerializeField] AnimationCurve distributionCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.1f);
        
        [BoxGroup("Tile Generation")]
        [InfoBox("Even Distribution: All levels have equal probability\n" +
                 "Low Level Bias: Favors levels 0-1 based on bias value\n" +
                 "Weighted Curve: Uses animation curve for custom distribution\n" +
                 "Only Low Levels: Only spawns levels 0-1\n" +
                 "Balanced Random: 30% low, 50% mid, 20% high levels", InfoMessageType.Info)]
        [ShowInInspector, ReadOnly]
        private string tileGenerationInfo = "See InfoBox above for distribution details.";

        [BoxGroup("Tile Generation Debug")]
        [Button("Test Distribution (100 samples)")]
        [GUIColor(0.7f, 0.7f, 1f)]
        private void TestDistribution()
        {
            var results = new Dictionary<int, int>();
            int samples = 100;
            
            for (int i = 0; i < samples; i++)
            {
                int level = GetStrategicLevel();
                results[level] = results.GetValueOrDefault(level, 0) + 1;
            }
            
            Debug.Log($"Distribution test for {distributionMode} ({samples} samples):");
            foreach (var kvp in results.OrderBy(x => x.Key))
            {
                float percentage = (kvp.Value / (float)samples) * 100f;
                Debug.Log($"Level {kvp.Key}: {kvp.Value} times ({percentage:F1}%)");
            }
        }
        
        [BoxGroup("Tile Generation Debug")]
        [Button("Reset Board with New Distribution")]
        [GUIColor(1f, 0.7f, 0.7f)]
        private void ResetBoardWithNewDistribution()
        {
            if (Application.isPlaying && gameBoard != null)
            {
                StartCoroutine(ResetBoardCoroutine());
            }
        }
        
        private IEnumerator ResetBoardCoroutine()
        {
            // Destroy all current tiles
            var tiles = new List<GameTile>(gameBoard.GameTiles);
            foreach (var tile in tiles)
            {
                if (tile != null && !tile.IsDestroyed)
                    tile.Pop();
            }
            
            yield return new WaitForSeconds(1.1f);
            
            // Respawn with new distribution
            gameBoardSpawner.Spawn();
            ApplySurvivalSettingsToBoard();
        }

        private void Awake()
        {
            selection = new LineSelection(lineRenderer);
            gameBoardSpawner = new DefaultGameBoardSpawner(gameBoard);
            gameOverCheck = new DefaultGameOverCheck(gameBoard);
            
            // Setup operations - we'll modify the existing MergeSelection behavior
            operations.Clear();
            operations.Add(new ProcessInput(gameBoard, selection));
            operations.Add(new SurvivalMergeSelection(gameBoard, selection));
            operations.Add(new ProcessVerticalMovement(gameBoard));
            operations.Add(new FillEmptyCells(gameBoard));
            
            tileTracker = GameObject.FindAnyObjectByType<GameTileTracker>();
        }

        void Start()
        {
            questSystem.OnQuestChanged += (Quest quest) =>
            {
                questCompleted = false;
            };

            questSystem.OnQuestCompleted += OnQuestCompleted;
            questSystem.OnNewDayStarted += OnNewDayStarted;
            
            // Subscribe to quest reward events
            questSystem.OnScoreReward += OnScoreReward;
            questSystem.OnTimeBonus += OnTimeBonus;

            survivalTimer.OnTimerEnd += OnTimerEnd;
            
            // Subscribe to survival resources events (will be uncommented when SurvivalResources is ready)
            if (survivalResources != null)
            {
                survivalResources.OnResourcesDepleted += OnResourcesDepleted;
            }
        }

        public override IEnumerator StartGame()
        {
            Score.Reset();
            GameStats.Reset();
            gameBoardSpawner.Spawn();
            
            // Apply survival mode settings to initial tiles
            ApplySurvivalSettingsToBoard();
            
            questSystem.StartNewDay();
            
            // Clear any accumulated time bonuses from previous games
            survivalTimer.ClearAccumulatedBonus();
            
            // Reset survival resources (will be uncommented when SurvivalResources is ready)
            if (survivalResources != null)
            {
                survivalResources.ResetResources();
                survivalResources.StartResourceDecay();
            }
            
            // Use quest-specific time limit if available, otherwise default to 60 seconds
            float timeLimit = questSystem.GetCurrentQuestTimeLimit();
            if (timeLimit <= 0) timeLimit = 60f; // Default fallback
            
            survivalTimer.StartTimer(timeLimit);
            questCompleted = false;
            resourcesDepleted = false;
            yield return null;
        }

        private IEnumerator WaitForInputOrTimesUp()
        {
            while (!timesUp)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) // or your input logic
                    break;
                yield return null;
            }
        }

        public override IEnumerator RunGame()
        {
            survivalTimer.OnTimerEnd += OnTimerEnd;
            questSystem.OnQuestCompleted += OnQuestCompleted;
            while (true)
            {
                // Unlimited mode: reset board if no moves and not in waiting state
                if (unlimitedMode && gameOverCheck.IsGameOver())
                {
                    NotificationEvents.Send(new NotificationMessage("No moves left"));
                    yield return new WaitForSeconds(1f);
                    // Temporarily disable quest tracking
                    if (tileTracker != null) tileTracker.ignoreTracking = true;
                    
                    // Destroy all tiles
                    var tiles = new List<GameTile>(gameBoard.GameTiles);
                    foreach (var tile in tiles)
                        if (tile != null && !tile.IsDestroyed)
                            tile.Pop();

                    // Wait for tiles to disappear
                    yield return new WaitForSeconds(1.1f);
                    
                    // Refill board
                    gameBoardSpawner.Spawn();
                    
                    // Apply survival mode settings to new tiles
                    ApplySurvivalSettingsToBoard();
                    
                    // Wait for tile spawn animations
                    yield return new WaitForTileMovement(gameBoard);
                    yield return new WaitForSeconds(0.5f);

                    NotificationEvents.Send(new NotificationMessage("Board reset!"));
                    
                    // Re-enable quest tracking after reset
                    if (tileTracker != null) tileTracker.ignoreTracking = false;
                    continue; // restart loop
                }

                yield return WaitForInputOrTimesUp();
                if (timesUp || resourcesDepleted)
                {
                    if (timesUp) Debug.Log("Time's up! Game over.");
                    if (resourcesDepleted) Debug.Log("Resources depleted! Game over.");
                    break;
                }
                yield return operations.Execute();
                
                // If quest completed, show progression UI and wait for player
                if (questCompleted)
                {
                    Debug.Log("Quest completed! Waiting for next day.");
                    waitingForNextDay = true;
                    
                    // Wait for player to click Next button
                    while (waitingForNextDay)
                    {
                        yield return null;
                        
                        if (timesUp || resourcesDepleted)
                        {
                            waitingForNextDay = false;
                            questCompleted = false;
                            break; // Exit the waiting loop
                        }
                        
                        if (questSystem.AllQuestsCompleted)
                        {
                            waitingForNextDay = false;
                            break; // Exit the waiting loop
                        }
                    }
                    
                    if (timesUp || resourcesDepleted)
                    {
                        if (timesUp) Debug.Log("Time's up! Game over.");
                        if (resourcesDepleted) Debug.Log("Resources depleted! Game over.");
                        break;
                    }
                    if (questSystem.AllQuestsCompleted)
                    {
                        Debug.Log("All quests completed! Game over. No more days left.");
                        break;
                    }
                    
                    // Continue to next iteration of the main loop after day progression
                    continue;
                }
            }
        }

        public override IEnumerator EndGame()
        {
            survivalTimer.StopTimer();
            survivalTimer.OnTimerEnd -= OnTimerEnd;
            questSystem.OnQuestCompleted -= OnQuestCompleted;
            questSystem.OnNewDayStarted -= OnNewDayStarted;
            questSystem.OnScoreReward -= OnScoreReward;
            questSystem.OnTimeBonus -= OnTimeBonus;
            GameEvents<UIEventType>.Trigger(UIEventType.GameOver);
            yield return null;
        }

        // Modify OnQuestCompleted to only set questCompleted = true and stop timer
        void OnQuestCompleted()
        {
            questCompleted = true;
            survivalTimer.StopTimer();
        }

        void OnTimerEnd()
        {
            // Game over logic here
            timesUp = true;
        }

        void OnResourcesDepleted()
        {
            // Game over due to resource depletion
            resourcesDepleted = true;
            Debug.Log("Game over - Resources depleted!");
        }

        void OnScoreReward(int scoreReward)
        {
            Score.Add(scoreReward);
            NotificationEvents.Send(new NotificationMessage($"Quest Bonus: +{scoreReward} points!"));
        }

        void OnTimeBonus(float timeBonus)
        {
            survivalTimer.AddTimeBonusForNextQuest(timeBonus);
            NotificationEvents.Send(new NotificationMessage($"Next Quest Bonus: +{timeBonus:F0} seconds!"));
        }

        void OnNewDayStarted()
        {
            // Restart timer for new quest with accumulated bonuses and quest-specific time limit
            // Skip only if this is the very first call during game initialization
            if (questCompleted || questSystem.Day > 1)
            {
                float timeLimit = questSystem.GetCurrentQuestTimeLimit();
                if (timeLimit <= 0) timeLimit = 60f; // Default fallback
                
                // Read the bonus BEFORE calling StartTimer (which resets it to 0)
                float bonusTime = survivalTimer.AccumulatedTimeBonus;
                survivalTimer.StartTimer(timeLimit);
                questCompleted = false;
                waitingForNextDay = false; // Reset waiting flag
                
                Debug.Log($"New quest started with time limit: {timeLimit + bonusTime:F1} seconds (base: {timeLimit}, bonus: {bonusTime:F1})");
            }
        }

        /// <summary>
        /// Call this method when the player clicks the "Next" button in the UI
        /// </summary>
        public void OnNextButtonClicked()
        {
            if (waitingForNextDay && questCompleted)
            {
                // Progress to next day when player clicks Next
                if (!questSystem.AllQuestsCompleted && !timesUp)
                {
                    questSystem.NextDay();
                    Debug.Log($"Player clicked Next - Moving to Day {questSystem.Day}");
                }
            }
        }

        /// <summary>
        /// Applies survival mode settings to all tiles on the board
        /// </summary>
        private void ApplySurvivalSettingsToBoard()
        {
            // Small delay to ensure all tiles are properly instantiated
            StartCoroutine(ApplySurvivalSettingsDelayed());
        }

        private IEnumerator ApplySurvivalSettingsDelayed()
        {
            yield return new WaitForEndOfFrame();
            
            foreach (var tile in gameBoard.GameTiles)
            {
                if (tile is DiceGameTile diceTile && !diceTile.IsDestroyed)
                {
                    // Set strategic levels for better gameplay
                    diceTile.CurrentLevel = GetStrategicLevel();
                }
            }
        }

        /// <summary>
        /// Returns a level based on the selected distribution mode
        /// </summary>
        private int GetStrategicLevel()
        {
            // Get the actual maximum level from the dice system dynamically
            int actualMaxLevel = GetDynamicMaxLevel();
            int maxLevel = Mathf.Min(maxTileLevel, actualMaxLevel);

            return distributionMode switch
            {
                TileDistributionMode.EvenDistribution => GetEvenDistributionLevel(maxLevel),
                TileDistributionMode.LowLevelBias => GetLowLevelBiasLevel(maxLevel),
                TileDistributionMode.WeightedCurve => GetWeightedCurveLevel(maxLevel),
                TileDistributionMode.OnlyLowLevels => GetOnlyLowLevelsLevel(),
                TileDistributionMode.BalancedRandom => GetBalancedRandomLevel(maxLevel),
                _ => GetEvenDistributionLevel(maxLevel)
            };
        }

        private int GetEvenDistributionLevel(int maxLevel)
        {
            // Equal probability for all levels from 0 to maxLevel
            return Random.Range(0, maxLevel + 1);
        }

        private int GetLowLevelBiasLevel(int maxLevel)
        {
            // Original low-level bias logic
            float randomValue = Random.value;
            
            if (randomValue < lowLevelBias)
            {
                // Heavily favor levels 0-1 for easy combinations
                return Random.Range(0, 2);
            }
            else
            {
                // Occasionally spawn higher levels
                return Random.Range(0, maxLevel + 1);
            }
        }

        private int GetWeightedCurveLevel(int maxLevel)
        {
            // Use animation curve to determine probability
            float randomValue = Random.value;
            float normalizedPosition = 0f;
            
            // Find the level that corresponds to this random value using the curve
            for (int level = 0; level <= maxLevel; level++)
            {
                float normalizedLevel = (float)level / maxLevel;
                float curveValue = distributionCurve.Evaluate(normalizedLevel);
                normalizedPosition += curveValue;
                
                if (randomValue <= normalizedPosition / GetCurveTotalArea())
                {
                    return level;
                }
            }
            
            return maxLevel; // Fallback
        }

        private int GetOnlyLowLevelsLevel()
        {
            // Only spawn levels 0 and 1
            return Random.Range(0, 2);
        }

        private int GetBalancedRandomLevel(int maxLevel)
        {
            // Slight preference for middle levels
            float randomValue = Random.value;
            
            if (randomValue < 0.3f)
            {
                // 30% chance for low levels (0-1)
                return Random.Range(0, Mathf.Min(2, maxLevel + 1));
            }
            else if (randomValue < 0.8f)
            {
                // 50% chance for mid levels
                int midStart = Mathf.Max(1, maxLevel / 3);
                int midEnd = Mathf.Min(maxLevel, (maxLevel * 2) / 3);
                return Random.Range(midStart, midEnd + 1);
            }
            else
            {
                // 20% chance for higher levels
                int highStart = Mathf.Max(1, (maxLevel * 2) / 3);
                return Random.Range(highStart, maxLevel + 1);
            }
        }

        private float GetCurveTotalArea()
        {
            // Calculate the total area under the curve for proper normalization
            float total = 0f;
            int samples = 100;
            
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                total += distributionCurve.Evaluate(t);
            }
            
            return total;
        }

        /// <summary>
        /// Gets the actual maximum level from the dice system dynamically
        /// </summary>
        private int GetDynamicMaxLevel()
        {
            // Try to find a dice tile on the board to get the real max level
            foreach (var tile in gameBoard.GameTiles)
            {
                if (tile is DiceGameTile diceTile && !diceTile.IsDestroyed)
                {
                    return diceTile.MaxLevel;
                }
            }
            
            // Use DiceLevelManager for max level
            if (DiceLevelManager.Instance != null)
            {
                return DiceLevelManager.Instance.MaxLevel;
            }
            
            // Final fallback: use the configured maxTileLevel
            return maxTileLevel;
        }
    }
}
