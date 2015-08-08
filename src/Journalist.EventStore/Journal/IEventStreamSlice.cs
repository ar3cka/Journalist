using System.Collections.Generic;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Journal
{
    public interface IEventStreamSlice : IReadOnlyCollection<JournaledEvent>
    {
        StreamVersion FromStreamVersion { get; }

        StreamVersion ToStreamVersion { get; }
    }
}
