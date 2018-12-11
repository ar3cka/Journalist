using System.Collections.Generic;
using Journalist.EventStore.Notifications.Channels;

namespace Journalist.EventStore.Notifications.Processing
{
    public interface IReceivedNotificationProcessor
    {
        void Process(IReceivedNotification notification);

        void RegisterHandlers(IEnumerable<INotificationHandler> handlers);

        int ProcessingCount { get; }
    }
}
