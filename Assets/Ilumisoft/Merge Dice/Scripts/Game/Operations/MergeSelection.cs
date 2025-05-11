using Ilumisoft.MergeDice.Events;
using Ilumisoft.MergeDice.Notifications;
using System.Collections;
using UnityEngine;

namespace Ilumisoft.MergeDice.Operations
{
    public class MergeSelection : IOperation
    {
        IGameBoard gameBoard;
        ISelection selection;
        IValidator selectionValidator;

        public MergeSelection(IGameBoard gameBoard, ISelection selection)
        {
            this.gameBoard = gameBoard;
            this.selection = selection;
            this.selectionValidator = new SelectionValidator(selection);
        }

        int MaxReachedLevel
        {
            get => GameStats.MaxReachedLevel;
            set => GameStats.MaxReachedLevel = value;
        }

        public IEnumerator Execute()
        {
            // Cancel if the selection is not valid
            if (selectionValidator.IsValid == false)
            {
                selection.Clear();
                yield break;
            }

            var last = selection.GetLast();

            ClearSelectionLine();

            GameEvents<SFXEventType>.Trigger(SFXEventType.Merge);

            MoveSelected(last.transform.position);

            IncreaseScore();

            yield return new WaitForTileMovement(gameBoard);

            LevelUp(last);

            GameEvents<SFXEventType>.Trigger(SFXEventType.Pop);

            PopSelected();

            selection.Clear();

            yield return new WaitForSeconds(0.2f);
        }

        private void LevelUp(GameTile gameTile)
        {
            if (gameTile is ICanLevelUp canLevelUp)
            {
                if (canLevelUp.HasMaxLevel)
                {
                    NotificationEvents.Send(new NotificationMessage()
                    {
                        Content = "Merged Six"
                    });

                    GameEvents<SFXEventType>.Trigger(SFXEventType.MergedSix);
                }
                else
                {
                    canLevelUp.LevelUp();
                    MaxReachedLevel = Mathf.Max(canLevelUp.CurrentLevel, MaxReachedLevel);
                    selection.Remove(gameTile);
                }
            }
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
            Score.Add(new ScoreRevenue(selection).GetValue());
        }

        private void MoveSelected(Vector3 position)
        {
            for (int i = 0; i < selection.Count - 1; i++)
            {
                var gameTile = selection.Get(i);

                if (gameTile is ICanMoveTo canMoveTo)
                {
                    canMoveTo.MoveTo(position, 0.2f);
                }
            }
        }

        private void PopSelected()
        {
            for (int i = 0; i < selection.Count; i++)
            {
                var gameTile = selection.Get(i);

                gameTile.Pop();
            }
        }
    }
}