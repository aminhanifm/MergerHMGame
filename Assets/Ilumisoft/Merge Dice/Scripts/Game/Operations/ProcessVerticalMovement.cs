using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ilumisoft.MergeDice.Operations
{
    public class ProcessVerticalMovement : IOperation
    {
        GameBoard gameBoard;

        List<GameObject> processedGameTiles;

        public ProcessVerticalMovement(GameBoard gameBoard)
        {
            this.gameBoard = gameBoard;
            processedGameTiles = new List<GameObject>();
        }

        public IEnumerator Execute()
        {
            PushGameTilesDown();

            yield return new WaitForTileMovement(gameBoard);
        }

        /// <summary>
        /// Makes GameTiles fall downwards if there are empty cells under them
        /// </summary>
        void PushGameTilesDown()
        {
            processedGameTiles.Clear();

            for (int x = 0; x < gameBoard.Width; x++)
            {
                PushGameTilesInColumnDown(x);
            }
        }

        /// <summary>
        /// Makes all GameTiles in the given column fall downwards if there are empty cells under them
        /// </summary>
        void PushGameTilesInColumnDown(int column)
        {
            for (int y = 0; y < gameBoard.Height; y++)
            {
                if (IsCellEmptyOrProcessed(column, y))
                {
                    if (TryGetNextUpperGameTile(column, y, out GameTile gameTile))
                    {
                        if (gameTile is ICanMoveTo canMoveTo)
                        {
                            canMoveTo.MoveTo(gameBoard.GetPosition(column, y));
                        }

                        processedGameTiles.Add(gameTile.gameObject);
                    }
                    //We can stop if no upper gameTile exists
                    else
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the given cell is empty or already processed. If a cell has been processed
        /// we can consider it as empty, because the gameTile inisde the cell will be moving downwards
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        bool IsCellEmptyOrProcessed(int x, int y)
        {
            var raycast = new GameTileRaycast(gameBoard.GetPosition(x, y), Vector2.zero, 0);

            if (TryGetGameTile(gameBoard.GetPosition(x, y), out var gameTile))
            {
                // Has the gameTile already been processed? => Ignore cell
                if (processedGameTiles.Contains(gameTile.gameObject))
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Tries to find the next upper active gameTile which has not already been processed
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="gameTile"></param>
        /// <returns></returns>
        bool TryGetNextUpperGameTile(int x, int y, out GameTile gameTile)
        {
            gameTile = null;

            for (int h = y + 1; h <= gameBoard.Height; h++)
            {
                if (TryGetGameTile(gameBoard.GetPosition(x, h), out gameTile))
                {
                    // Skip destroyed or already processed gameTiles
                    if (gameTile.IsDestroyed || processedGameTiles.Contains(gameTile.gameObject))
                    {
                        continue;
                    }

                    return true;
                }
            }

            return false;
        }

        bool TryGetGameTile(Vector2 position, out GameTile gameTile)
        {
            var raycast = new GameTileRaycast(position, Vector2.zero, 0);

            return raycast.Perform(out gameTile);
        }
    }
}