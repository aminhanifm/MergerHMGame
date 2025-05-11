namespace Ilumisoft.MergeDice
{
    public class GameBoardMovement
    {
        IGameBoard gameBoard;

        public GameBoardMovement(IGameBoard gameBoard)
        {
            this.gameBoard = gameBoard;
        }

        public bool IsAnyGameTileMoving()
        {
            foreach (var gameTile in gameBoard.GameTiles)
            {
                if (IsMoving(gameTile))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsMoving(GameTile gameTile)
        {
            if (gameTile is ICanMoveTo canMoveTo)
            {
                return canMoveTo.IsMoving;
            }

            return false;
        }
    }
}