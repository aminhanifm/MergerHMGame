using Ilumisoft.MergeDice.Events;
using Ilumisoft.MergeDice.Operations;
using Ilumisoft.MergeDice;
using System.Collections;
using UnityEngine;

namespace Ilumisoft.MergeDice.Survival
{
    public class SurvivalMergeSelection : IOperation
    {
        IGameBoard gameBoard;
        ISelection selection;
        IValidator selectionValidator;

        public SurvivalMergeSelection(IGameBoard gameBoard, ISelection selection)
        {
            this.gameBoard = gameBoard;
            this.selection = selection;
            this.selectionValidator = new SelectionValidator(selection);
        }

        public IEnumerator Execute()
        {
            // Cancel if the selection is not valid
            if (selectionValidator.IsValid == false)
            {
                selection.Clear();
                yield break;
            }

            // Progress quest for all selected tiles BEFORE destroying them
            ProgressQuestForSelection();

            ClearSelectionLine();

            GameEvents<SFXEventType>.Trigger(SFXEventType.Merge);

            // Move all tiles to center position for visual effect
            var centerPosition = CalculateCenterPosition();
            MoveSelected(centerPosition);

            IncreaseScore();

            yield return new WaitForTileMovement(gameBoard);

            // In survival mode, destroy ALL selected tiles instead of leveling up
            GameEvents<SFXEventType>.Trigger(SFXEventType.Pop);

            DestroyAllSelected();

            selection.Clear();

            yield return new WaitForSeconds(0.2f);
        }

        private Vector3 CalculateCenterPosition()
        {
            Vector3 center = Vector3.zero;
            for (int i = 0; i < selection.Count; i++)
            {
                center += selection.Get(i).transform.position;
            }
            return center / selection.Count;
        }

        private void ClearSelectionLine()
        {
            if (selection is LineSelection lineSelection)
            {
                lineSelection.ClearLine();
            }
        }

        private void IncreaseScore()
        {
            // Score based on number of tiles merged
            int baseScore = selection.Count * 10;
            
            // Bonus for larger merges
            if (selection.Count >= 4)
                baseScore *= 2;
            if (selection.Count >= 6)
                baseScore *= 2;

            Score.Add(baseScore);
        }

        private void MoveSelected(Vector3 position)
        {
            for (int i = 0; i < selection.Count; i++)
            {
                var gameTile = selection.Get(i);

                if (gameTile is ICanMoveTo canMoveTo)
                {
                    canMoveTo.MoveTo(position, 0.2f);
                }
            }
        }

        private void DestroyAllSelected()
        {
            for (int i = 0; i < selection.Count; i++)
            {
                var gameTile = selection.Get(i);
                gameTile.Pop();
            }
        }

        private void ProgressQuestForSelection()
        {
            var questTracker = Object.FindFirstObjectByType<GameTileTracker>();
            if (questTracker != null && !questTracker.ignoreTracking)
            {
                for (int i = 0; i < selection.Count; i++)
                {
                    var tile = selection.Get(i);
                    if (tile is DiceGameTile diceTile)
                    {
                        questTracker.QuestSystem?.ProgressQuest(diceTile.CurrentLevel, 1);
                    }
                }
            }
        }
    }
}
