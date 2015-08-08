using System.Collections.Generic;
using Journalist.EventStore.Events;

namespace Journalist.EventStore.Journal
{
    public interface IEventStreamSlice : IReadOnlyCollection<JournaledEvent>
    {
        StreamVersion SliceSteamVersion { get; }

        bool EndOfStream { get; }
    }
}
