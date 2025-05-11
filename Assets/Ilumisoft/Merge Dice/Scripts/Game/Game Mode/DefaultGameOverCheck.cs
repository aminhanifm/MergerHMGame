using UnityEngine;

namespace Ilumisoft.MergeDice
{
    public class DefaultGameOverCheck : IGameOverCheck
    {
        IGameGrid grid;
        SelectableCounter selectableCounter;

        public DefaultGameOverCheck(IGameGrid grid)
        {
            this.grid = grid;
            selectableCounter = new SelectableCounter(grid);
        }

        public bool IsGameOver()
        {
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    if (TryGetGameTile(grid.GetPosition(x, y), out GameTile gameTile))
                    {
                        if (IsSelectable(gameTile))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        bool TryGetGameTile(Vector2 position, out GameTile gameTile)
        {
            var raycast = new GameTileRaycast(position, Vector2.zero, 0);

            return raycast.Perform(out gameTile);
        }

        bool IsSelectable(GameTile gameTile)
        {
            return selectableCounter.Count(gameTile)>=GameRules.MinSelectionSize;
        }
    }
}