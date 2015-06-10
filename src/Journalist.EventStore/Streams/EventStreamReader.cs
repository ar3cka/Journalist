using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Journal.StreamCursor;
using Journalist.EventStore.Streams.Serializers;
using Journalist.Extensions;

namespace Journalist.EventStore.Streams
{
    public class EventStreamReader : IEventStreamReader
    {
        private readonly string m_streamName;
        private readonly EventStreamCursor m_streamCursor;
        private readonly IEventSerializer m_serializer;

        private List<object> m_readedEvents;

        public EventStreamReader(
            string streamName,
            EventStreamCursor streamCursor,
            IEventSerializer serializer)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(streamCursor, "streamCursor");
            Require.NotNull(serializer, "serializer");

            m_streamName = streamName;
            m_streamCursor = streamCursor;
            m_serializer = serializer;
        }

        public async Task ReadEventsAsync()
        {
            if (m_streamCursor.EndOfStream)
            {
                throw new InvalidOperationException("Stream \"{0}\" is empty.".FormatString(m_streamName));
            }

            await m_streamCursor.FetchSlice();

            m_readedEvents = new List<object>(m_streamCursor.Slice.Count);
            foreach (var journaledEvent in m_streamCursor.Slice)
            {
                using (var payloadReader = new StreamReader(journaledEvent.EventPayload))
                {
                    m_readedEvents.Add(m_serializer.Deserialize(
                        payloadReader,
                        journaledEvent.EventType));
                }
            }
        }

        public IReadOnlyList<object> Events
        {
            get
            {
                if (m_readedEvents != null)
                {
                    return m_readedEvents;
                }

                return EmptyArray.Get<object>();
            }
        }

        public bool HasMoreEvents
        {
            get { return !m_streamCursor.EndOfStream; }
        }

        public string StreamName
        {
            get { return m_streamName; }
        }
    }
}
