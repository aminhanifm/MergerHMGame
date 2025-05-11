using UnityEngine.Events;

namespace Ilumisoft.MergeDice.Events
{
    public static class GameEvents<T>
    {
        public static UnityAction<T> OnTrigger = null;

        public static void Trigger(T eventType)
        {
            OnTrigger?.Invoke(eventType);
        }
    }
}