using System.Collections.Generic;
using UnityEngine;

namespace Ilumisoft.MergeDice
{
    public class SelectableCounter
    {
        IGameGrid grid;
        IConnectionValidator connectionValidator;

        Vector2[] directions;

        public SelectableCounter(IGameGrid grid)
        {
            this.grid = grid;

            this.connectionValidator = new LevelValidator();

            directions = new Vector2[]
            {
                Vector2.up,
                Vector2.up + Vector2.right,
                Vector2.right,
                Vector2.down + Vector2.right,
                Vector2.down,
                Vector2.down + Vector2.left,
                Vector2.left,
                Vector2.up + Vector2.left
            };
        }

        /// <summary>
        /// Returns the number of all GameTiles which are selectable from the given one
        /// </summary>
        /// <param name="gameTile"></param>
        /// <returns></returns>
        public int Count(GameTile gameTile)
        {
            List<GameTile> result = new List<GameTile>();

            if (gameTile != null && gameTile.IsDestroyed == false)
            {
                AddToSelection(result, gameTile);
            }

            return result.Count;
        }

        public void AddToSelection(List<GameTile> result, GameTile gameTile)
        {
            if (result.Contains(gameTile))
            {
                return;
            }

            result.Add(gameTile);

            gameTile.DisableCollider();

            foreach (var direction in directions)
            {
                TrySelect(result, gameTile, direction);
            }

            gameTile.EnableCollider();
        }

        public void TrySelect(List<GameTile> result, GameTile gameTile, Vector2 direction)
        {
            float maxDistance = Mathf.Sqrt(2 * grid.CellSize * grid.CellSize);

            var raycast = new GameTileRaycast(gameTile.transform.position, direction, maxDistance);

            if (raycast.Perform(out var neighbor))
            {
                if (connectionValidator.IsValid(gameTile, neighbor))
                {
                    AddToSelection(result, neighbor);
                }
            }
        }
    }
}