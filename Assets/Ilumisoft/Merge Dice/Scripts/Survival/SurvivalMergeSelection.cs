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

            // Check for food and water dice BEFORE processing
            CheckForResourceDice();

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
                // Store requirement progress before updating
                var questProgress = questTracker.QuestSystem?.CurrentQuestProgress;
                var requirementsBefore = new int[questProgress?.requirements.Length ?? 0];
                
                if (questProgress != null)
                {
                    for (int i = 0; i < questProgress.requirements.Length; i++)
                    {
                        requirementsBefore[i] = questProgress.requirements[i].currentAmount;
                    }
                }

                // Progress the quest for each tile
                for (int i = 0; i < selection.Count; i++)
                {
                    var tile = selection.Get(i);
                    if (tile is DiceGameTile diceTile)
                    {
                        questTracker.QuestSystem?.ProgressQuest(diceTile.CurrentLevel, 1);
                    }
                }

                // Check if any requirements were completed after the update
                if (questProgress != null)
                {
                    bool anyRequirementCompleted = false;
                    
                    for (int i = 0; i < questProgress.requirements.Length; i++)
                    {
                        int currentAmount = questProgress.requirements[i].currentAmount;
                        int targetAmount = questProgress.requirements[i].targetAmount;
                        int previousAmount = requirementsBefore[i];
                        
                        // Check if this requirement was just completed
                        if (previousAmount < targetAmount && currentAmount >= targetAmount)
                        {
                            anyRequirementCompleted = true;
                            Debug.Log($"Quest requirement completed: Level {questProgress.requirements[i].tileLevel} ({currentAmount}/{targetAmount})");
                            break;
                        }
                    }

                    // Play special SFX if any requirement was completed
                    if (anyRequirementCompleted)
                    {
                        GameEvents<SFXEventType>.Trigger(SFXEventType.MergedSix);
                    }
                }
            }
        }

        private void CheckForResourceDice()
        {
            // Find the survival game mode to get dice level configuration
            var survivalGameMode = Object.FindFirstObjectByType<SurvivalGameMode>();
            if (survivalGameMode == null) return;

            // For now, just log the resource dice found (will be enhanced when SurvivalResources is integrated)
            int foodDiceCount = 0;
            int waterDiceCount = 0;
            int foodLevel = survivalGameMode.foodDiceLevel;
            int waterLevel = survivalGameMode.waterDiceLevel;

            // Check each selected tile
            for (int i = 0; i < selection.Count; i++)
            {
                var tile = selection.Get(i);
                if (tile is DiceGameTile diceTile)
                {
                    int level = diceTile.CurrentLevel;

                    if (level == foodLevel)
                    {
                        foodDiceCount++;
                    }
                    else if (level == waterLevel)
                    {
                        waterDiceCount++;
                    }
                }
            }

            // Log resource dice merging (will be replaced with actual resource system)
            if (foodDiceCount > 0)
            {
                Debug.Log($"Food dice merged: {foodDiceCount} dice at level {foodLevel}");
                survivalGameMode.SurvivalResources.OnFoodDiceMerged(foodLevel, foodDiceCount);
            }

            if (waterDiceCount > 0)
            {
                Debug.Log($"Water dice merged: {waterDiceCount} dice at level {waterLevel}");
                survivalGameMode.SurvivalResources.OnWaterDiceMerged(waterLevel, waterDiceCount);
            }
        }
    }
}
