using System.Collections.Generic;

namespace Ilumisoft.MergeDice
{
    public class GameTileManager : SingletonBehaviour<GameTileManager>
    {
        public List<GameTile> GameTiles { get; } = new List<GameTile>();

        public static event System.Action<GameTile> OnGameTileRegistered;

        public void Register(GameTile gameTile)
        {
            gameTile.OnTileDestroyed += OnGameTileDestroy;
            GameTiles.Add(gameTile);
            OnGameTileRegistered?.Invoke(gameTile);
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