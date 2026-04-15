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
        bool gameRunning = false; // Flag to track if game is actively running
        [BoxGroup("Game State")]
        bool showingDayIntro = false; // Flag to track if day intro is being shown
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
        public TileDistributionMode distributionMode = TileDistributionMode.EvenDistribution;
        
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
        [ShowInInspector, ReadOnly]
        private bool enableDistributionDebug = false;

        [BoxGroup("Tile Generation Debug")]
        [Button("Toggle Debug Logging")]
        [GUIColor(0.7f, 1f, 0.7f)]
        private void ToggleDebugLogging()
        {
            // Toggle debug logging for distribution
            enableDistributionDebug = !enableDistributionDebug;
            Debug.Log($"Distribution debug logging: {(enableDistributionDebug ? "ENABLED" : "DISABLED")}");
        }

        [BoxGroup("Tile Generation Debug")]
        [Button("Test Distribution (1000 samples)")]
        [GUIColor(0.7f, 0.7f, 1f)]
        private void TestDistribution()
        {
            var results = new Dictionary<int, int>();
            int samples = 1000;
            
            // Test the actual distribution method being used
            int actualMaxLevel = GetDynamicMaxLevel();
            int maxLevel = Mathf.Min(maxTileLevel, actualMaxLevel);
            
            Debug.Log($"Testing {distributionMode} with maxLevel: {maxLevel} (actualMaxLevel: {actualMaxLevel}, maxTileLevel: {maxTileLevel})");
            
            for (int i = 0; i < samples; i++)
            {
                int level = GetStrategicLevel();
                results[level] = results.GetValueOrDefault(level, 0) + 1;
            }
            
            Debug.Log($"Distribution test for {distributionMode} ({samples} samples):");
            float expectedPercentage = 100f / (maxLevel + 1);
            Debug.Log($"Expected percentage per level (if even): {expectedPercentage:F1}%");
            
            foreach (var kvp in results.OrderBy(x => x.Key))
            {
                float percentage = (kvp.Value / (float)samples) * 100f;
                float deviation = Mathf.Abs(percentage - expectedPercentage);
                Debug.Log($"Level {kvp.Key}: {kvp.Value} times ({percentage:F1}%) - Deviation: {deviation:F1}%");
            }
            
            // Also test the raw Random.Range method directly
            Debug.Log("\n--- Direct Random.Range test ---");
            var directResults = new Dictionary<int, int>();
            for (int i = 0; i < samples; i++)
            {
                int level = Random.Range(0, maxLevel + 1);
                directResults[level] = directResults.GetValueOrDefault(level, 0) + 1;
            }
            
            Debug.Log($"Direct Random.Range(0, {maxLevel + 1}) test:");
            foreach (var kvp in directResults.OrderBy(x => x.Key))
            {
                float percentage = (kvp.Value / (float)samples) * 100f;
                float deviation = Mathf.Abs(percentage - expectedPercentage);
                Debug.Log($"Level {kvp.Key}: {kvp.Value} times ({percentage:F1}%) - Deviation: {deviation:F1}%");
            }
        }

        [BoxGroup("Tile Generation Debug")]
        [Button("Test New Tile Distribution")]
        [GUIColor(0.7f, 1f, 1f)]
        private void TestNewTileDistribution()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("This test only works in play mode!");
                return;
            }

            var factory = FindAnyObjectByType<SurvivalGameTileFactory>();
            if (factory == null)
            {
                Debug.LogError("SurvivalGameTileFactory not found!");
                return;
            }

            var results = new Dictionary<int, int>();
            int samples = 100;

            Debug.Log($"Testing new tile spawning distribution ({samples} samples):");
            Debug.Log($"Current distribution mode: {distributionMode}");

            for (int i = 0; i < samples; i++)
            {
                int level = GetDistributedLevel();
                results[level] = results.GetValueOrDefault(level, 0) + 1;
            }

            int actualMaxLevel = GetDynamicMaxLevel();
            int maxLevel = Mathf.Min(maxTileLevel, actualMaxLevel);
            float expectedPercentage = 100f / (maxLevel + 1);

            Debug.Log($"Results (maxLevel: {maxLevel}, expected per level if even: {expectedPercentage:F1}%):");
            foreach (var kvp in results.OrderBy(x => x.Key))
            {
                float percentage = (kvp.Value / (float)samples) * 100f;
                Debug.Log($"Level {kvp.Key}: {kvp.Value} times ({percentage:F1}%)");
            }

            Debug.Log("This distribution will now be used for new tiles spawned after merges!");
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
            
            // Get the survival game tile factory
            var survivalFactory = gameBoard.GetComponent<SurvivalGameTileFactory>();
            if (survivalFactory == null)
            {
                survivalFactory = FindAnyObjectByType<SurvivalGameTileFactory>();
            }
            
            // Setup operations - use SurvivalFillEmptyCells instead of regular FillEmptyCells
            operations.Clear();
            operations.Add(new ProcessInput(gameBoard, selection));
            operations.Add(new SurvivalMergeSelection(gameBoard, selection));
            operations.Add(new ProcessVerticalMovement(gameBoard));
            if (survivalFactory != null)
            {
                operations.Add(new SurvivalFillEmptyCells(gameBoard, survivalFactory));
            }
            else
            {
                Debug.LogError("SurvivalGameTileFactory not found! Using regular FillEmptyCells as fallback.");
                operations.Add(new FillEmptyCells(gameBoard));
            }
            
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

        void Update()
        {
            // Only run these checks when the game is actively running
            if (!gameRunning) return;

            // Handle quest completion independently of operations
            if (questCompleted && !waitingForNextDay)
            {
                Debug.Log("Quest completed! Waiting for next day.");
                waitingForNextDay = true;
            }

            // Handle day progression when player clicks Next
            if (waitingForNextDay && questCompleted)
            {
                // The OnNextButtonClicked method will handle the actual progression
                // This just ensures we stay in waiting state until then
                return;
            }

            // If showing day intro, don't run normal game logic
            if (showingDayIntro)
            {
                // Day intro is being shown, wait for player to click "Start Day"
                return;
            }

            // Check for game over conditions
            if (timesUp || resourcesDepleted)
            {
                if (timesUp) Debug.Log("Time's up! Game over.");
                if (resourcesDepleted) Debug.Log("Resources depleted! Game over.");
                gameRunning = false;
                StartCoroutine(EndGame());
                return;
            }

            // Check if all quests are completed
            if (questSystem.AllQuestsCompleted)
            {
                Debug.Log("All quests completed! Game over. No more days left.");
                gameRunning = false;
                StartCoroutine(EndGame());
                return;
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
            
            // Reset survival resources but DON'T start decay yet (wait for intro to complete)
            if (survivalResources != null)
            {
                survivalResources.ResetResources();
                // survivalResources.StartResourceDecay(); // Moved to OnDayIntroCompleted
            }
            
            // DON'T start the timer yet - wait for day intro to complete
            // Use quest-specific time limit if available, otherwise default to 60 seconds
            // float timeLimit = questSystem.GetCurrentQuestTimeLimit();
            // if (timeLimit <= 0) timeLimit = 60f; // Default fallback
            // survivalTimer.StartTimer(timeLimit); // Moved to OnDayIntroCompleted
            
            questCompleted = false;
            resourcesDepleted = false;
            timesUp = false;
            waitingForNextDay = false;
            showingDayIntro = true; // Start with day intro showing
            gameRunning = true; // Start the game logic in Update
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
            
            while (gameRunning)
            {
                // Unlimited mode: reset board if no moves and not in waiting state or showing intro
                if (unlimitedMode && gameOverCheck.IsGameOver() && !waitingForNextDay && !showingDayIntro)
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

                // Only wait for input and execute operations if not waiting for next day and not showing intro
                if (!waitingForNextDay && !showingDayIntro)
                {
                    yield return WaitForInputOrTimesUp();
                    if (gameRunning) // Check if game is still running after waiting
                    {
                        yield return operations.Execute();
                    }
                }
                else
                {
                    // When waiting for next day or showing intro, just yield and let Update handle the logic
                    yield return null;
                }
            }
        }

        public override IEnumerator EndGame()
        {
            gameRunning = false; // Stop the Update logic
            survivalTimer.StopTimer();
            survivalResources.StopResourceDecay();
            survivalTimer.OnTimerEnd -= OnTimerEnd;
            questSystem.OnQuestCompleted -= OnQuestCompleted;
            questSystem.OnNewDayStarted -= OnNewDayStarted;
            questSystem.OnScoreReward -= OnScoreReward;
            questSystem.OnTimeBonus -= OnTimeBonus;
            GameEvents<UIEventType>.Trigger(UIEventType.GameOver);
            yield return null;
        }

        public void TriggerFinalDayGameOver()
        {
            if (!gameRunning)
            {
                return;
            }

            questCompleted = false;
            waitingForNextDay = false;
            showingDayIntro = false;
            StartCoroutine(EndGame());
        }

        // Modify OnQuestCompleted to only set questCompleted = true and stop timer
        void OnQuestCompleted()
        {
            questCompleted = true;
            survivalTimer.StopTimer();
            survivalResources.StopResourceDecay();
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
            // DON'T start timer here anymore - the day intro will handle it
            // Just log that a new day started and show intro flag
            if (questCompleted || questSystem.Day > 1)
            {
                showingDayIntro = true; // Show intro for new day
                Debug.Log($"New day started: Day {questSystem.Day} - Showing intro");
            }
        }

        /// <summary>
        /// Call this method when the player clicks the "Next" button in the UI
        /// </summary>
        public void OnNextButtonClicked()
        {
            if (waitingForNextDay && questCompleted && gameRunning)
            {
                // Progress to next day when player clicks Next
                if (!questSystem.AllQuestsCompleted && !timesUp && !resourcesDepleted)
                {
                    questSystem.NextDay();
                    Debug.Log($"Player clicked Next - Moving to Day {questSystem.Day}");
                    
                    // Set flag to show day intro instead of immediately starting gameplay
                    showingDayIntro = true;
                    waitingForNextDay = false;
                    questCompleted = false; // This will be set again when next quest completes
                }
            }
        }

        /// <summary>
        /// Call this method when the day intro is completed and player clicks "Start Day"
        /// </summary>
        public void OnDayIntroCompleted()
        {
            if (showingDayIntro && gameRunning)
            {
                showingDayIntro = false;
                
                // NOW start the timer for the current quest
                float timeLimit = questSystem.GetCurrentQuestTimeLimit();
                if (timeLimit <= 0) timeLimit = survivalTimer.TimeLimit; // Default fallback
                
                // Read any accumulated bonus time before starting
                float bonusTime = survivalTimer.AccumulatedTimeBonus;
                survivalTimer.StartTimer(timeLimit);
                
                // Start survival resources decay
                if (survivalResources != null)
                {
                    survivalResources.StartResourceDecay();
                }
                
                Debug.Log($"Day {questSystem.Day} intro completed - Starting gameplay with {timeLimit + bonusTime:F1}s timer (base: {timeLimit}, bonus: {bonusTime:F1})");
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
            List<int> spawnableLevels = GetSpawnableLevels(maxLevel);

            if (enableDistributionDebug)
            {
                Debug.Log($"GetStrategicLevel: distributionMode={distributionMode}, maxLevel={maxLevel} (actualMax={actualMaxLevel}, tileMax={maxTileLevel}, spawnableCount={spawnableLevels.Count})");
            }

            if (spawnableLevels.Count == 0)
            {
                Debug.LogWarning("No spawnable survival levels are available. Falling back to level 0.");
                return 0;
            }

            return distributionMode switch
            {
                TileDistributionMode.EvenDistribution => GetEvenDistributionLevel(spawnableLevels),
                TileDistributionMode.LowLevelBias => GetLowLevelBiasLevel(spawnableLevels),
                TileDistributionMode.WeightedCurve => GetWeightedCurveLevel(spawnableLevels),
                TileDistributionMode.OnlyLowLevels => GetOnlyLowLevelsLevel(spawnableLevels),
                TileDistributionMode.BalancedRandom => GetBalancedRandomLevel(spawnableLevels),
                _ => GetEvenDistributionLevel(spawnableLevels)
            };
        }

        /// <summary>
        /// Public method for external systems (like SurvivalGameTileFactory) to get strategic levels
        /// using the same distribution logic as the game mode
        /// </summary>
        public int GetDistributedLevel()
        {
            return GetStrategicLevel();
        }

        private List<int> GetSpawnableLevels(int maxLevel)
        {
            List<int> levels = new List<int>();

            for (int level = 0; level <= maxLevel; level++)
            {
                if (IsLevelAllowedForSpawn(level))
                {
                    levels.Add(level);
                }
            }

            return levels;
        }

        private bool IsLevelAllowedForSpawn(int level)
        {
            if (survivalResources == null || survivalResources.AreMechanicsEnabled)
            {
                return true;
            }

            return level != foodDiceLevel && level != waterDiceLevel;
        }

        private int GetRandomLevelFromList(IReadOnlyList<int> levels)
        {
            return levels[Random.Range(0, levels.Count)];
        }

        private List<int> GetLevelsInRange(IReadOnlyList<int> levels, int minLevel, int maxLevel)
        {
            List<int> filteredLevels = new List<int>();

            foreach (int level in levels)
            {
                if (level >= minLevel && level <= maxLevel)
                {
                    filteredLevels.Add(level);
                }
            }

            return filteredLevels;
        }

        private int GetEvenDistributionLevel(IReadOnlyList<int> spawnableLevels)
        {
            int level = GetRandomLevelFromList(spawnableLevels);
            
            // Debug logging for troubleshooting
            if (enableDistributionDebug)
            {
                Debug.Log($"EvenDistribution: generated level={level}");
            }
            
            return level;
        }

        private int GetLowLevelBiasLevel(IReadOnlyList<int> spawnableLevels)
        {
            // Original low-level bias logic
            float randomValue = Random.value;
            List<int> lowLevels = GetLevelsInRange(spawnableLevels, 0, 1);
            
            if (enableDistributionDebug)
            {
                Debug.Log($"GetLowLevelBiasLevel: randomValue={randomValue:F3}, lowLevelBias={lowLevelBias:F3}");
            }
            
            if (randomValue < lowLevelBias)
            {
                // Heavily favor levels 0-1 for easy combinations
                int level = GetRandomLevelFromList(lowLevels.Count > 0 ? lowLevels : spawnableLevels);
                if (enableDistributionDebug)
                {
                    Debug.Log($"GetLowLevelBiasLevel: Using low bias, generated level {level}");
                }
                return level;
            }
            else
            {
                // Occasionally spawn higher levels
                int level = GetRandomLevelFromList(spawnableLevels);
                if (enableDistributionDebug)
                {
                    Debug.Log($"GetLowLevelBiasLevel: Using high levels, generated level {level}");
                }
                return level;
            }
        }

        private int GetWeightedCurveLevel(IReadOnlyList<int> spawnableLevels)
        {
            float totalWeight = 0f;

            foreach (int level in spawnableLevels)
            {
                float normalizedLevel = spawnableLevels.Count == 1 ? 0f : (float)level / Mathf.Max(1, spawnableLevels[spawnableLevels.Count - 1]);
                totalWeight += Mathf.Max(0f, distributionCurve.Evaluate(normalizedLevel));
            }

            if (totalWeight <= 0f)
            {
                return GetRandomLevelFromList(spawnableLevels);
            }

            float randomValue = Random.value * totalWeight;
            float currentWeight = 0f;

            foreach (int level in spawnableLevels)
            {
                float normalizedLevel = spawnableLevels.Count == 1 ? 0f : (float)level / Mathf.Max(1, spawnableLevels[spawnableLevels.Count - 1]);
                currentWeight += Mathf.Max(0f, distributionCurve.Evaluate(normalizedLevel));

                if (randomValue <= currentWeight)
                {
                    return level;
                }
            }

            return spawnableLevels[spawnableLevels.Count - 1];
        }

        private int GetOnlyLowLevelsLevel(IReadOnlyList<int> spawnableLevels)
        {
            List<int> lowLevels = GetLevelsInRange(spawnableLevels, 0, 1);
            return GetRandomLevelFromList(lowLevels.Count > 0 ? lowLevels : spawnableLevels);
        }

        private int GetBalancedRandomLevel(IReadOnlyList<int> spawnableLevels)
        {
            // Slight preference for middle levels
            float randomValue = Random.value;
            int maxSpawnableLevel = spawnableLevels[spawnableLevels.Count - 1];
            List<int> lowLevels = GetLevelsInRange(spawnableLevels, 0, 1);
            int midStart = Mathf.Max(1, maxSpawnableLevel / 3);
            int midEnd = Mathf.Min(maxSpawnableLevel, (maxSpawnableLevel * 2) / 3);
            int highStart = Mathf.Max(1, (maxSpawnableLevel * 2) / 3);

            List<int> midLevels = GetLevelsInRange(spawnableLevels, midStart, midEnd);
            List<int> highLevels = GetLevelsInRange(spawnableLevels, highStart, maxSpawnableLevel);
            
            if (randomValue < 0.3f)
            {
                return GetRandomLevelFromList(lowLevels.Count > 0 ? lowLevels : spawnableLevels);
            }
            else if (randomValue < 0.8f)
            {
                return GetRandomLevelFromList(midLevels.Count > 0 ? midLevels : spawnableLevels);
            }
            else
            {
                return GetRandomLevelFromList(highLevels.Count > 0 ? highLevels : spawnableLevels);
            }
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
                    if (enableDistributionDebug)
                        Debug.Log($"Found dice tile with MaxLevel: {diceTile.MaxLevel}");
                    return diceTile.MaxLevel;
                }
            }
            
            // Use DiceLevelManager for max level
            if (DiceLevelManager.Instance != null)
            {
                if (enableDistributionDebug)
                    Debug.Log($"Using DiceLevelManager MaxLevel: {DiceLevelManager.Instance.MaxLevel}");
                return DiceLevelManager.Instance.MaxLevel;
            }
            
            // Final fallback: use the configured maxTileLevel
            if (enableDistributionDebug)
                Debug.Log($"Using fallback maxTileLevel: {maxTileLevel}");
            return maxTileLevel;
        }
    }
}
