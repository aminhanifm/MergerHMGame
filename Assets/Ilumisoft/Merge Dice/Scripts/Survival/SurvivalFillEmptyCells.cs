using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ilumisoft.MergeDice.Operations;

namespace Ilumisoft.MergeDice.Survival
{
    public class SurvivalFillEmptyCells : IOperation
    {
        GameBoard gameBoard; // Change to GameBoard for proper scaling
        SurvivalGameTileFactory survivalFactory;

        public SurvivalFillEmptyCells(GameBoard gameBoard, SurvivalGameTileFactory factory)
        {
            this.gameBoard = gameBoard;
            this.survivalFactory = factory;
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

            for (int x = 0; x < gameBoard.Width; x++)
            {
                for (int y = 0; y < gameBoard.Height; y++)
                {
                    var raycast = new GameTileRaycast(gameBoard.GetPosition(x, y), Vector2.zero, 0);

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
            Vector3 position = gameBoard.GetPosition(cell.x, cell.y);
            
            Debug.Log($"Spawning tile at {position} in SurvivalFillEmptyCells");
            
            // Use the survival factory directly - it now handles scaling and distribution
            var tile = survivalFactory.Spawn(position);
            
            // The level and scaling are already set by the factory's Spawn method
        }
    }
}
