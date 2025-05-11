using UnityEngine;

namespace Ilumisoft.MergeDice.Notifications
{
    public class NotificationManager : MonoBehaviour
    {
        [SerializeField]
        NotificationBase notificationPrefab = null;

        private void OnEnable()
        {
            NotificationEvents.OnSend += CreateNotification;
        }

        private void OnDisable()
        {
            NotificationEvents.OnSend -= CreateNotification;
        }

        private void CreateNotification(INotificationMessage notification)
        {
            if (notificationPrefab != null)
            {
                var instance = Instantiate(notificationPrefab, transform);
                instance.Content = notification.Content;
            }
            else
            {
                Debug.LogError("You have no notification prefab defined", this);
            }
        }
    }
}