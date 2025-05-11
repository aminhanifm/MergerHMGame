namespace Ilumisoft.MergeDice.Notifications
{
    public struct NotificationMessage : INotificationMessage
    {
        public NotificationMessage(string content)
        {
            this.Content = content;
        }

        public string Content { get; set; }
    }
}