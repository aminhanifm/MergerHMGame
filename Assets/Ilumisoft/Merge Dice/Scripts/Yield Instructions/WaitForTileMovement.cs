using UnityEngine;

namespace Ilumisoft.MergeDice
{
    public class WaitForTileMovement : CustomYieldInstruction
    {
        GameBoardMovement boardMovement;

        public WaitForTileMovement(IGameBoard gameBoard)
        {
            this.boardMovement = new GameBoardMovement(gameBoard);
        }

        public override bool keepWaiting
        {
            get
            {
                return boardMovement.IsAnyGameTileMoving();
            }
        }
    }
}