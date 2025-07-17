using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ilumisoft.MergeDice.Operations
{
    public class FillEmptyCells : IOperation
    {
        IGameGrid grid;
        IGameTileFactory gameTileFactory;

        public FillEmptyCells(GameBoard gameBoard)
        {
            this.grid = gameBoard;
            this.gameTileFactory = gameBoard;
        }

        public IEnumerator Execute()
        {
            List<Vector2Int> emptyCells = FindEmptyCells();

            SpawnCells(emptyCells);

            yield return new WaitForSeconds(0.25f);
        }

        List<Vector2Int> FindEmptyCells()
        {
            List<Vector2Int> emptyCells = new List<Vector2Int>();

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var raycast = new GameTileRaycast(grid.GetPosition(x, y), Vector2.zero, 0);

                    if (!raycast.Perform(out _))
                    {
                        emptyCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            return emptyCells;
        }

        void SpawnCells(List<Vector2Int> cells)
        {
            foreach (var cell in cells)
            {
                SpawnCell(cell);
            }
        }

        void SpawnCell(Vector2Int cell)
        {
            Vector3 position = grid.GetPosition(cell.x, cell.y);

            var newTile = gameTileFactory.Spawn(position);
            
            // Apply survival mode settings if we're in survival mode
            var survivalMode = Object.FindFirstObjectByType<Survival.SurvivalGameMode>();
            if (survivalMode != null && newTile is DiceGameTile diceTile)
            {
                // Use survival-friendly level generation
                diceTile.CurrentLevel = GetSurvivalRandomLevel();
            }
        }

        /// <summary>
        /// Returns a random level optimized for survival mode gameplay
        /// </summary>
        private int GetSurvivalRandomLevel()
        {
            // Get the actual maximum level dynamically
            int actualMaxLevel = GetDynamicMaxLevel();
            
            // 70% chance for levels 0-1 (easy to combine)
            if (Random.value < 0.7f)
            {
                return Random.Range(0, 2);
            }
            // 30% chance for higher levels but respect actual max
            else
            {
                return Random.Range(2, Mathf.Min(6, actualMaxLevel + 1));
            }
        }

        /// <summary>
        /// Gets the actual maximum level from the dice system dynamically
        /// </summary>
        private int GetDynamicMaxLevel()
        {
            // Try to find a dice tile to get the real max level
            var allTiles = Object.FindObjectsByType<DiceGameTile>(FindObjectsSortMode.None);
            if (allTiles.Length > 0)
            {
                return allTiles[0].MaxLevel;
            }
            
            // Use DiceLevelManager for max level
            if (DiceLevelManager.Instance != null)
            {
                return DiceLevelManager.Instance.MaxLevel;
            }
            
            // Final fallback
            return 6;
        }
    }
}