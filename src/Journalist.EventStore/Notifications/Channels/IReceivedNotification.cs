using System.Threading.Tasks;

namespace Journalist.EventStore.Notifications.Channels
{
    public interface IReceivedNotification
    {
        Task RetryAsync();

        Task CompleteAsync();

        INotification Notification { get; }
    }
}
