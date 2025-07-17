using UnityEngine;
using Ilumisoft.MergeDice;

namespace Ilumisoft.MergeDice.Survival
{
    public class GameTileTracker : MonoBehaviour
    {
        [SerializeField] QuestSystem questSystem;

        public QuestSystem QuestSystem
        {
            get => questSystem;
        }

        public bool ignoreTracking = false;

        private void OnEnable()
        {
            StartCoroutine(EnsureSubscription());
        }

        private System.Collections.IEnumerator EnsureSubscription()
        {
            while (GameTileManager.Instance == null)
            {
                yield return null;
            }
            GameTileManager.Instance.GameTiles.ForEach(tile => tile.OnTileDestroyed += OnTileDestroyed);
            GameTileManager.OnGameTileRegistered += SubscribeToTile;
        }

        private void OnDisable()
        {
            if (GameTileManager.Instance != null)
            {
                GameTileManager.Instance.GameTiles.ForEach(tile => tile.OnTileDestroyed -= OnTileDestroyed);
                GameTileManager.OnGameTileRegistered -= SubscribeToTile;
            }
        }

        private void SubscribeToTile(GameTile tile)
        {
            tile.OnTileDestroyed += OnTileDestroyed;
        }

        private void OnTileDestroyed(GameTile tile)
        {
            // if (ignoreTracking) return;
            // // Check if tile matches any quest requirement
            // var quest = questSystem.CurrentQuest;
            // if (quest != null && !quest.IsComplete)
            // {
            //     if (tile is DiceGameTile diceTile)
            //     {
            //         questSystem.ProgressQuest(diceTile.CurrentLevel, 1);
            //     }
            // }
        }
    }
}
