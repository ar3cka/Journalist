using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;
using Journalist.Extensions;

namespace Journalist.EventStore.Streams
{
    public class EventStreamReader : IEventStreamReader
    {
        private readonly string m_streamName;
        private readonly Func<StreamVersion, Task<IEventStreamCursor>> m_openCursor;

        private IEventStreamCursor m_streamCursor;
        private List<JournaledEvent> m_readedEvents;

        public EventStreamReader(
            string streamName,
            IEventStreamCursor streamCursor,
            Func<StreamVersion, Task<IEventStreamCursor>> openCursor)
        {
            Require.NotEmpty(streamName, "streamName");
            Require.NotNull(streamCursor, "streamCursor");
            Require.NotNull(openCursor, "openCursor");

            m_streamName = streamName;
            m_streamCursor = streamCursor;
            m_openCursor = openCursor;
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

        public async Task ContinueAsync()
        {
            if (IsCompleted)
            {
                m_streamCursor = await m_openCursor(CurrentStreamVersion.Increment());
                m_readedEvents = null;
                return;
            }

            throw new InvalidOperationException("Reader is not in competed state.");
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

        public StreamVersion CurrentStreamVersion
        {
            get { return m_streamCursor.CurrentVersion; }
        }

        public bool HasEvents
        {
            get { return !m_streamCursor.EndOfStream; }
        }

        public string StreamName
        {
            get { return m_streamName; }
        }

        public bool IsCompleted
        {
            get { return m_streamCursor.EndOfStream; }
        }

        public bool IsInitial
        {
            get { return !m_streamCursor.Fetching && !m_streamCursor.EndOfStream; }
        }

        public bool IsReading
        {
            get { return m_streamCursor.Fetching; }
        }
    }
}
