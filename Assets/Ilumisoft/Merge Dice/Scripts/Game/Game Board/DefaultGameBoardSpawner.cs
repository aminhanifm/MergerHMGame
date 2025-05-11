namespace Ilumisoft.MergeDice
{
    public class DefaultGameBoardSpawner : ISpawner
    {
        IGameGrid grid;
        IGameTileFactory gameTileFactory;

        public DefaultGameBoardSpawner(IGameBoard gameBoard)
        {
            this.grid = gameBoard;
            this.gameTileFactory = gameBoard;
        }

        public void Spawn()
        {
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var position = grid.GetPosition(x, y);
                    gameTileFactory.Spawn(position);
                }
            }
        }
    }
}