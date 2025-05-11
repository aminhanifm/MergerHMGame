using UnityEngine.Events;

namespace Ilumisoft.MergeDice.Notifications
{
    public static class NotificationEvents
    {
        public static UnityAction<INotificationMessage> OnSend = null;

        public static void Send(INotificationMessage notification)
        {
            OnSend?.Invoke(notification);
        }
    }
}