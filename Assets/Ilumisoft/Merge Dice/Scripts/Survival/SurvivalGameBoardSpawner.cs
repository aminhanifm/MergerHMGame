using UnityEngine;
using Ilumisoft.MergeDice;

namespace Ilumisoft.MergeDice.Survival
{
    public class SurvivalGameBoardSpawner : ISpawner
    {
        IGameGrid grid;
        SurvivalGameTileFactory survivalFactory;

        public SurvivalGameBoardSpawner(IGameBoard gameBoard, SurvivalGameTileFactory factory)
        {
            this.grid = gameBoard;
            this.survivalFactory = factory;
        }

        public void Spawn()
        {
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var position = grid.GetPosition(x, y);
                    survivalFactory.Spawn(position);
                }
            }
        }
    }
}
