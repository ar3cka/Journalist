using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore;
using Journalist.EventStore.Events;
using Journalist.EventStore.Streams;
using Journalist.Tasks;

namespace Journalist.EventSourced.Application.UnitTests.Infrastructure.Stubs
{
    public class EventStreamReaderStub : IEventStreamReader
    {
        private readonly StreamVersion m_streamVersion;

        private JournaledEvent[] m_events;

        public EventStreamReaderStub(JournaledEvent[] events)
        {
            m_events = events;
            m_streamVersion = events.Length == 0
                ? StreamVersion.Unknown
                : StreamVersion.Create(events.Count());
        }

        public Task ReadEventsAsync()
        {
            IsInitial = false;
            IsReading = true;

            return TaskDone.Done;
        }

        public Task ContinueAsync()
        {
            return TaskDone.Done;
        }

        public IReadOnlyList<JournaledEvent> Events
        {
            get
            {
                var tmp = m_events;
                m_events = EmptyArray.Get<JournaledEvent>();

                return tmp;
            }
        }

        public bool HasEvents
        {
            get { return m_events.Length != 0; }
        }

        public string StreamName
        {
            get; set;
        }

        public bool IsInitial
        {
            get; private set;
        }

        public bool IsReading
        {
            get;
            private set;
        }

        public bool IsCompleted { get; set; }

        public StreamVersion CurrentStreamVersion
        {
            get { return m_streamVersion; }
        }
    }
}