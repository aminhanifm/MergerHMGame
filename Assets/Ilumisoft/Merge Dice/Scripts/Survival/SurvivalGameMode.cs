using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ilumisoft.MergeDice;
using Ilumisoft.MergeDice.Operations;
using Ilumisoft.MergeDice.Events;
using Ilumisoft.MergeDice.Notifications;

namespace Ilumisoft.MergeDice.Survival
{
    public class SurvivalGameMode : GameMode
    {
        [SerializeField] GameBoard gameBoard = null;
        [SerializeField] SelectionLineRenderer lineRenderer = null;
        [SerializeField] SurvivalTimer survivalTimer = null;
        [SerializeField] QuestSystem questSystem = null;

        ISelection selection;
        OperationQueue operations = new OperationQueue();
        bool questCompleted = false;
        bool timesUp = false;
        GameTileTracker tileTracker; // Add tracker reference
        ISpawner gameBoardSpawner;
        IGameOverCheck gameOverCheck;

        public bool unlimitedMode = false; // allow unlimited resets

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
            tileTracker = GameObject.FindAnyObjectByType<GameTileTracker>();
        }

        void Start()
        {
            questSystem.OnQuestChanged += (Quest quest) =>
            {
                questCompleted = false;
            };

            survivalTimer.OnTimerEnd += OnTimerEnd;
        }

        public override IEnumerator StartGame()
        {
            Score.Reset();
            GameStats.Reset();
            gameBoardSpawner.Spawn();
            questSystem.StartNewDay();
            survivalTimer.TimeLimit = 60;
            survivalTimer.StartTimer();
            questCompleted = false;
            yield return null;
        }

        private IEnumerator WaitForInputOrTimesUp()
        {
            while (!timesUp)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) // or your input logic
                    break;
                yield return null;
            }
        }

        public override IEnumerator RunGame()
        {
            survivalTimer.OnTimerEnd += OnTimerEnd;
            questSystem.OnQuestCompleted += OnQuestCompleted;
            bool waitingForNextDay = false;
            while (true)
            {
                // Unlimited mode: reset board if no moves and not in waiting state
                if (unlimitedMode && gameOverCheck.IsGameOver())
                {
                    NotificationEvents.Send(new NotificationMessage("No moves left"));
                    yield return new WaitForSeconds(1f);
                    // Temporarily disable quest tracking
                    if (tileTracker != null) tileTracker.ignoreTracking = true;
                    
                    // Destroy all tiles
                    var tiles = new List<GameTile>(gameBoard.GameTiles);
                    foreach (var tile in tiles)
                        if (tile != null && !tile.IsDestroyed)
                            tile.Pop();

                    // Wait for tiles to disappear
                    yield return new WaitForSeconds(1.1f);
                    // Refill board
                    gameBoardSpawner.Spawn();
                    // Wait for tile spawn animations
                    yield return new WaitForTileMovement(gameBoard);
                    yield return new WaitForSeconds(0.5f);

                    NotificationEvents.Send(new NotificationMessage("Board reset!"));
                    
                    // Re-enable quest tracking after reset
                    if (tileTracker != null) tileTracker.ignoreTracking = false;
                    continue; // restart loop
                }

                yield return WaitForInputOrTimesUp();
                if (timesUp)
                {
                    Debug.Log("Time's up! Game over.");
                    break;
                }
                yield return operations.Execute();
                // If quest completed, show progression UI and wait for player
                if (questCompleted)
                {
                    Debug.Log("Quest completed! Waiting for next day.");
                    waitingForNextDay = true;
                    while (waitingForNextDay)
                    {
                        yield return null;
                        if (!questCompleted || questSystem.AllQuestsCompleted || timesUp)
                        {
                            waitingForNextDay = false;
                        }
                        if (timesUp)
                        {
                            questCompleted = false; // Ensure exit
                        }
                    }
                    if (timesUp)
                    {
                        Debug.Log("Time's up! Game over.");
                        break;
                    }
                    if (questSystem.AllQuestsCompleted)
                    {
                        Debug.Log("All quests completed! Game over. No more days left.");
                        break;
                    }
                }
            }
        }

        public override IEnumerator EndGame()
        {
            survivalTimer.StopTimer();
            survivalTimer.OnTimerEnd -= OnTimerEnd;
            questSystem.OnQuestCompleted -= OnQuestCompleted;
            GameEvents<UIEventType>.Trigger(UIEventType.GameOver);
            yield return null;
        }

        // Modify OnQuestCompleted to only set questCompleted = true and stop timer
        void OnQuestCompleted()
        {
            questCompleted = true;
            survivalTimer.StopTimer();
        }

        void OnTimerEnd()
        {
            // Game over logic here
            timesUp = true;
        }
    }
}
