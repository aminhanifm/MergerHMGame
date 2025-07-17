using System;
using UnityEngine;

namespace Ilumisoft.MergeDice.Survival
{
    public class SurvivalTimer : MonoBehaviour
    {
        public float TimeLimit = 60f;
        public float TimeLeft { get; private set; }
        public bool IsRunning { get; private set; }

        public event Action OnTimerEnd;
        public event Action<float> OnTimerTick;

        public void StartTimer()
        {
            TimeLeft = TimeLimit;
            IsRunning = true;
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
