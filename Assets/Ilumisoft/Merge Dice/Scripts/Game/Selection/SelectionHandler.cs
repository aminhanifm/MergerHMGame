using Ilumisoft.MergeDice.Events;
using System.Collections.Generic;

namespace Ilumisoft.MergeDice
{
    public class SelectionHandler : ISelectionHandler
    {
        ISelection selection;

        List<IConnectionValidator> connectionValidators;

        public SelectionHandler(IGameGrid grid, ISelection selection)
        {
            this.selection = selection;

            this.connectionValidators = new List<IConnectionValidator>()
            {
                new LevelValidator(),
                new GridDistanceValidator(grid)
            };
        }

        public void HandleSelection(GameTile gameTile)
        {
            if (IsSelectable(gameTile))
            {
                selection.Add(gameTile);

                if (selection.Count > 1)
                {
                    GameEvents<SFXEventType>.Trigger(SFXEventType.Select);
                }
            }
            else if (IsSecondToLast(gameTile))
            {
                selection.Remove(selection.GetLast());
            }
        }

        bool IsSelectable(GameTile gameTile)
        {
            if (selection.Contains(gameTile))
            {
                return false;
            }

            return selection.Count == 0 || AreConnectable(selection.GetLast(), gameTile);
        }

        bool IsSecondToLast(GameTile gameTile)
        {
            return selection.Count > 1 && selection.Get(selection.Count - 2) == gameTile;
        }

        public bool AreConnectable(GameTile gameTile1, GameTile gameTile2)
        {
            // If any connection validation fails we consider the two game tiles as not connectable...
            foreach (var validator in connectionValidators)
            {
                if (validator.IsValid(gameTile1, gameTile2) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}