using System.Collections.Generic;

namespace Ilumisoft.MergeDice
{
    public interface IGameBoard : IGameGrid, IGameTileFactory
    {
        IList<GameTile> GameTiles { get; }
    }
}