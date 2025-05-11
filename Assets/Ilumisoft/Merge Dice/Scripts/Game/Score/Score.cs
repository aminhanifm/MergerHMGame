using UnityEngine.Events;

namespace Ilumisoft.MergeDice
{
    public static class Score
    {
        public static UnityAction<int> OnScoreChanged = null;

        public static int Value;

        public static int Get()
        {
            return Value;
        }

        public static void Reset()
        {
            Value = 0;

            OnScoreChanged?.Invoke(Value);
        }

        public static void Add(int value)
        {
            Value += value;

            OnScoreChanged?.Invoke(Value);
        }
    }
}