using System;
using UnityEngine;

namespace Ilumisoft.MergeDice.Survival
{
    public class SurvivalTimer : MonoBehaviour
    {
        public float TimeLimit = 75f;
        public float TimeLeft { get; private set; }
        public bool IsRunning { get; private set; }
        public float AccumulatedTimeBonus { get; private set; }

        public event Action OnTimerEnd;
        public event Action<float> OnTimerTick;

        public void StartTimer()
        {
            TimeLeft = TimeLimit + AccumulatedTimeBonus;
            AccumulatedTimeBonus = 0f; // Reset after applying
            IsRunning = true;
        }

        public void StartTimer(float customTimeLimit)
        {
            TimeLimit = customTimeLimit;
            TimeLeft = TimeLimit + AccumulatedTimeBonus;
            AccumulatedTimeBonus = 0f; // Reset after applying
            IsRunning = true;
        }

        public void AddTimeBonusForNextQuest(float bonusSeconds)
        {
            AccumulatedTimeBonus += bonusSeconds;
            Debug.Log($"Time bonus accumulated for next quest: {bonusSeconds} seconds. Total bonus: {AccumulatedTimeBonus:F1}");
        }

        public void ClearAccumulatedBonus()
        {
            AccumulatedTimeBonus = 0f;
        }

        public void ResetTimer()
        {
            TimeLeft = TimeLimit;
            IsRunning = false;
        }

        public void StopTimer()
        {
            IsRunning = false;
        }

        private void Update()
        {
            if (!IsRunning) return;
            TimeLeft -= UnityEngine.Time.deltaTime;
            OnTimerTick?.Invoke(TimeLeft);
            if (TimeLeft <= 0f)
            {
                TimeLeft = 0f;
                IsRunning = false;
                OnTimerEnd?.Invoke();
            }
        }
    }
}
