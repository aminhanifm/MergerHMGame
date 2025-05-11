using System.Collections.Generic;

namespace Ilumisoft.MergeDice
{
    public class GameTileManager : SingletonBehaviour<GameTileManager>
    {
        public List<GameTile> GameTiles { get; } = new List<GameTile>();

        public void Register(GameTile gameTile)
        {
            gameTile.OnTileDestroyed += OnGameTileDestroy;
            GameTiles.Add(gameTile);
        }

        public void Deregister(GameTile gameTile)
        {
            gameTile.OnTileDestroyed -= OnGameTileDestroy;
            GameTiles.Remove(gameTile);
        }

        private void OnGameTileDestroy(GameTile gameTile)
        {
            Deregister(gameTile);
        }
    }
}