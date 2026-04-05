using System.Collections.Generic;
using UnityEngine;

namespace Ilumisoft.MergeDice
{
    public class GameBoard : GameGrid, IGameBoard
    {
        [SerializeField]
        AbstractGameTileFactory gameTileFactory = null;

        [SerializeField]
        float tileScale = 0.0f;

        public IList<GameTile> GameTiles => GameTileManager.Instance.GameTiles;

        public float TileScale => tileScale > 0.0f ? tileScale : CellSize;

        public GameTile Spawn(GameTile prefab, Vector3 position)
        {
            var gameTile = gameTileFactory.Spawn(prefab, position);

            return ApplyCellScale(gameTile);
        }

        public GameTile Spawn(Vector3 position)
        {
            var gameTile = gameTileFactory.Spawn(position);

            return ApplyCellScale(gameTile);
        }

        private GameTile ApplyCellScale(GameTile gameTile)
        {
            gameTile.transform.localScale = Vector3.one * TileScale;

            return gameTile;
        }
    }
}