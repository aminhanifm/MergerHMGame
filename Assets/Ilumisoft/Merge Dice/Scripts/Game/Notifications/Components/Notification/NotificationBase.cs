using UnityEngine;
using UnityEngine.Events;

namespace Ilumisoft.MergeDice.Notifications
{
    public class NotificationBase : MonoBehaviour
    {
        string content = string.Empty;

        public UnityAction OnContentChanged { get; set; } = null;

        public string Content
        {
            get => content;
            set
            {
                if (content != value)
                {
                    content = value;
                    OnContentChanged?.Invoke();
                }
            }
        }
    }
}