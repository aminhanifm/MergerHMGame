using UnityEngine;
using UnityEngine.UI;

namespace Ilumisoft.MergeDice.Notifications
{
    [RequireComponent(typeof(NotificationBase))]
    class NotificationPresenter : MonoBehaviour
    {
        [SerializeField]
        Canvas canvas = null;

        [SerializeField]
        Text textComponent = null;

        NotificationBase notificationBase = null;

        private void Awake()
        {
            notificationBase = GetComponent<NotificationBase>();
        }

        private void Start()
        {
            canvas.worldCamera = Camera.main;
        }

        private void Reset()
        {
            canvas = GetComponentInChildren<Canvas>();
            textComponent = GetComponentInChildren<Text>();
        }

        private void OnEnable()
        {
            if (notificationBase != null)
            {
                notificationBase.OnContentChanged += OnContentChanged;
            }
        }

        private void OnDisable()
        {
            if (notificationBase != null)
            {
                notificationBase.OnContentChanged -= OnContentChanged;
            }
        }

        private void OnContentChanged()
        {
            textComponent.text = notificationBase.Content;
        }
    }
}