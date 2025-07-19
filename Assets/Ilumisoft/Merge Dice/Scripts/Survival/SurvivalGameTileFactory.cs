using System.Collections.Generic;
using UnityEngine;
using Ilumisoft.MergeDice;

namespace Ilumisoft.MergeDice.Survival
{
    public class SurvivalGameTileFactory : AbstractGameTileFactory
    {
        [SerializeField]
        List<GameTile> prefabs = new List<GameTile>();

        [SerializeField]
        [Range(1, 4)]
        int maxRandomLevel = 3; // Limit random levels to keep tiles combinable

        [SerializeField]
        [Range(0f, 1f)]
        float lowLevelBias = 0.7f; // Probability to spawn lower level tiles

        GameTileManager GameTileManager => GameTileManager.Instance;

        GameObject gameTileContainer;
        
        // Reference to the survival game mode for distribution settings
        private SurvivalGameMode survivalGameMode;

        void Awake()
        {
            this.gameTileContainer = new GameObject("Survival Game Tiles");
            
            // Find the survival game mode
            survivalGameMode = FindAnyObjectByType<SurvivalGameMode>();
            if (survivalGameMode == null)
            {
                Debug.LogWarning("SurvivalGameTileFactory: Could not find SurvivalGameMode. Using fallback distribution.");
            }
        }

        public override GameTile Spawn(Vector3 position)
        {
            var prefab = prefabs.GetRandom();
            return Spawn(prefab, position);
        }

        public override GameTile Spawn(GameTile prefab, Vector3 position)
        {
            var gameTile = Instantiate(prefab, position, Quaternion.identity);
            gameTile.transform.SetParent(gameTileContainer.transform);

            // Set a strategic level for better gameplay
            if (gameTile is DiceGameTile diceTile)
            {
                int targetLevel = GetStrategicLevel();
                Debug.Log($"SurvivalGameTileFactory: Setting tile to level {targetLevel}");
                diceTile.CurrentLevel = targetLevel;
                Debug.Log($"SurvivalGameTileFactory: After setting, tile CurrentLevel = {diceTile.CurrentLevel}, MaxLevel = {diceTile.MaxLevel}");
            }

            // Apply proper cell scale like GameBoard does
            var gameBoard = FindAnyObjectByType<GameBoard>();
            if (gameBoard != null)
            {
                gameTile.transform.localScale = Vector3.one * gameBoard.CellSize;
            }
            else
            {
                Debug.LogWarning("SurvivalGameTileFactory: Could not find GameBoard to apply cell scale!");
            }

            GameTileManager.Register(gameTile);
            return gameTile;
        }

        /// <summary>
        /// Returns a level that promotes good gameplay by favoring lower levels
        /// and ensuring there are always tiles that can be combined
        /// </summary>
        private int GetStrategicLevel()
        {
            // If we have a reference to the survival game mode, use its distribution settings
            if (survivalGameMode != null)
            {
                // Use the same distribution logic as the game mode
                int level = survivalGameMode.GetDistributedLevel();
                Debug.Log($"SurvivalGameTileFactory: Using game mode distribution, got level {level}");
                return level;
            }
            
            Debug.Log("SurvivalGameTileFactory: No game mode reference, using fallback distribution");
            // Fallback to original logic if no game mode reference
            float randomValue = Random.value;
            
            if (randomValue < lowLevelBias)
            {
                // Heavily favor levels 0-1 for easy combinations
                int level = Random.Range(0, 2);
                Debug.Log($"SurvivalGameTileFactory: Fallback low bias, got level {level}");
                return level;
            }
            else
            {
                // Occasionally spawn higher levels but cap them
                int level = Random.Range(0, Mathf.Min(maxRandomLevel + 1, 5));
                Debug.Log($"SurvivalGameTileFactory: Fallback high level, got level {level}");
                return level;
            }
        }

        /// <summary>
        /// Analyzes current board state and ensures good tile distribution
        /// </summary>
        public int GetBalancedLevel()
        {
            // Count existing tiles by level
            Dictionary<int, int> levelCounts = new Dictionary<int, int>();
            
            if (GameTileManager.Instance != null)
            {
                foreach (var tile in GameTileManager.Instance.GameTiles)
                {
                    if (tile is DiceGameTile diceTile && !diceTile.IsDestroyed)
                    {
                        int level = diceTile.CurrentLevel;
                        levelCounts[level] = levelCounts.GetValueOrDefault(level, 0) + 1;
                    }
                }
            }

            // If we have very few low-level tiles, prioritize them
            int lowLevelCount = levelCounts.GetValueOrDefault(0, 0) + levelCounts.GetValueOrDefault(1, 0);
            if (lowLevelCount < 3)
            {
                return Random.Range(0, 2);
            }

            // Otherwise use normal strategic level
            return GetStrategicLevel();
        }
    }
}
