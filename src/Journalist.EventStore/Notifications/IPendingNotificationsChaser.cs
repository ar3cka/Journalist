namespace Journalist.EventStore.Notifications
{
    public interface IPendingNotificationsChaser
    {
        void Start();

        void Stop();
    }
}
