using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ilumisoft.MergeDice.Notifications
{
    [RequireComponent(typeof(NotificationBase))]
    class NotificationPresenter : MonoBehaviour
    {
        [SerializeField]
        Canvas canvas = null;

        [SerializeField]
        TMP_Text textComponent = null;

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
            textComponent = GetComponentInChildren<TMP_Text>();
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