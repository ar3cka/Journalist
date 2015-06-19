using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal.StreamCursor;
using Journalist.Extensions;

namespace Journalist.EventStore.Streams
{
    public class EventStreamReader : IEventStreamReader
    {
        private readonly string m_streamName;
        private readonly EventStreamCursor m_streamCursor;

        private List<JournaledEvent> m_readedEvents;

        public EventStreamReader(string streamName, EventStreamCursor streamCursor)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(streamCursor, "streamCursor");

            m_streamName = streamName;
            m_streamCursor = streamCursor;
        }

        public async Task ReadEventsAsync()
        {
            if (m_streamCursor.EndOfStream)
            {
                throw new InvalidOperationException("Stream \"{0}\" is empty.".FormatString(m_streamName));
            }

            await m_streamCursor.FetchSlice();

            m_readedEvents = new List<JournaledEvent>(m_streamCursor.Slice.Count);
            foreach (var journaledEvent in m_streamCursor.Slice)
            {
                m_readedEvents.Add(journaledEvent);
            }
        }

        public IReadOnlyList<JournaledEvent> Events
        {
            get
            {
                if (m_readedEvents != null)
                {
                    return m_readedEvents;
                }

                return EmptyArray.Get<JournaledEvent>();
            }
        }

        public bool HasEvents
        {
            get { return !m_streamCursor.EndOfStream; }
        }

        public string StreamName
        {
            get { return m_streamName; }
        }
    }
}
