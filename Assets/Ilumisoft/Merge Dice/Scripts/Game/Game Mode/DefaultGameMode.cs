using Ilumisoft.MergeDice.Events;
using Ilumisoft.MergeDice.Operations;
using Ilumisoft.MergeDice.Notifications; // Add this using directive for notifications
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Ilumisoft.MergeDice
{
    public class DefaultGameMode : GameMode
    {
        [SerializeField]
        GameBoard gameBoard = null;

        [SerializeField]
        SelectionLineRenderer lineRenderer = null;

        ISelection selection;

        IGameOverCheck gameOverCheck;

        ISpawner gameBoardSpawner;

        OperationQueue operations = new OperationQueue();

        public bool unlimitedMode = false; // Make this public for access

        private void Awake()
        {
            selection = new LineSelection(lineRenderer);
            gameBoardSpawner = new DefaultGameBoardSpawner(gameBoard);
            gameOverCheck = new DefaultGameOverCheck(gameBoard);

            operations.Clear();
            operations.Add(new ProcessInput(gameBoard, selection));
            operations.Add(new MergeSelection(gameBoard, selection));
            operations.Add(new ProcessVerticalMovement(gameBoard));
            operations.Add(new FillEmptyCells(gameBoard));
        }

        public override IEnumerator StartGame()
        {
            Score.Reset();

            GameStats.Reset();

            gameBoardSpawner.Spawn();

            yield return null;
        }

        public override IEnumerator RunGame()
        {
            while (IsGameOver() == false)
            {
                yield return new WaitForInput();
                yield return operations.Execute();
            }

            // If unlimited mode is enabled, erase all tiles and restart the game loop
            if (unlimitedMode)
            {
                // Send notification to player
                NotificationEvents.Send(new NotificationMessage("No moves left"));

                // Add slight delay before starting to clear board
                yield return new WaitForSeconds(1f);

                // Copy to a list to avoid modifying collection during iteration
                var tiles = new List<GameTile>(gameBoard.GameTiles);
                foreach (var tile in tiles)
                {
                    if (tile != null && !tile.IsDestroyed)
                        tile.Pop();
                }

                // Wait for all tiles to be destroyed and animations to complete
                yield return new WaitForSeconds(1.1f);

                // Refill the board
                gameBoardSpawner.Spawn();

                // Wait for tile spawn animations to complete
                yield return new WaitForTileMovement(gameBoard);
                yield return new WaitForSeconds(0.5f); // Add slight delay after animations

                // Send another notification
                NotificationEvents.Send(new NotificationMessage("Board reset!"));

                // Restart the game loop
                yield return RunGame();
                yield break;
            }

            yield return new WaitForSeconds(1);
        }

        public override IEnumerator EndGame()
        {
            GameEvents<UIEventType>.Trigger(UIEventType.GameOver);

            yield return null;
        }

        bool IsGameOver()
        {
            return gameOverCheck.IsGameOver();
        }
    }
}