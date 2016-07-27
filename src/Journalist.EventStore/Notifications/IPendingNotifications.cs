using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Notifications.Types;

namespace Journalist.EventStore.Notifications
{
    public interface IPendingNotifications
    {
        Task<IDictionary<string, List<EventStreamUpdated>>> LoadAsync();

        Task DeleteAsync(string streamName, StreamVersion streamVersion);

        Task DeleteAsync(string streamName, StreamVersion[] streamVersions);
    }
}
