using System.Collections.Generic;

namespace Journalist.EventStore.Notifications.Persistence
{
    public sealed class FailedNotification : IFailedNotification
    {
        public FailedNotification(string id, IReadOnlyDictionary<string, string> properties)
        {
            Require.NotEmpty(id, nameof(id));
            Require.NotNull(properties, nameof(properties));

            Id = id;
            Properties = properties;
        }

        public string Id { get; }

        public IReadOnlyDictionary<string, string> Properties { get; }
    }
}
