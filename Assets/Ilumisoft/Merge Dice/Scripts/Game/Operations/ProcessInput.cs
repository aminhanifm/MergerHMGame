using System.Collections;
using UnityEngine;

namespace Ilumisoft.MergeDice.Operations
{
    public class ProcessInput : IOperation
    {
        MouseInput mouseInput;
        ISelectionHandler selectionHandler;

        public ProcessInput(IGameGrid grid, ISelection selection)
        {
            selectionHandler = new SelectionHandler(grid, selection);

            mouseInput = new MouseInput(Camera.main);
        }

        public IEnumerator Execute()
        {
            while (mouseInput.IsLeftButtonDown())
            {
                if (mouseInput.TryGetHitObject<GameTile>(out var gameTile))
                {
                    selectionHandler.HandleSelection(gameTile);
                }

                yield return null;
            }
        }
    }
}