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

        void Awake()
        {
            this.gameTileContainer = new GameObject("Survival Game Tiles");
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
                diceTile.CurrentLevel = GetStrategicLevel();
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
            // Use weighted probability to favor lower levels
            float randomValue = Random.value;
            
            if (randomValue < lowLevelBias)
            {
                // Heavily favor levels 0-1 for easy combinations
                return Random.Range(0, 2);
            }
            else
            {
                // Occasionally spawn higher levels but cap them
                return Random.Range(0, Mathf.Min(maxRandomLevel + 1, 5));
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
