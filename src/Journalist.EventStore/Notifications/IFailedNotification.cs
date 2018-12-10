using System.Collections.Generic;

namespace Journalist.EventStore.Notifications
{
    public interface IFailedNotification
    {
        string Id { get; }

        IReadOnlyDictionary<string, string> Properties { get; }
    }
}
