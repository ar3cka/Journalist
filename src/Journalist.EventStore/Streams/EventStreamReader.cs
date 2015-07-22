using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Events;
using Journalist.EventStore.Events.Mutation;
using Journalist.EventStore.Journal;
using Journalist.Extensions;

namespace Journalist.EventStore.Streams
{
    public class EventStreamReader : EventStreamInteractionEntity, IEventStreamReader
    {
        private readonly Func<StreamVersion, Task<IEventStreamCursor>> m_openCursor;
        private readonly IEventMutationPipeline m_mutationPipeline;

        private List<JournaledEvent> m_readedEvents;
        private IEventStreamCursor m_streamCursor;

        public EventStreamReader(
            string streamName,
            IEventStoreConnectionState connectionState,
            IEventStreamCursor streamCursor,
            IEventMutationPipeline mutationPipeline,
            Func<StreamVersion, Task<IEventStreamCursor>> openCursor) : base(streamName, connectionState)
        {
            Require.NotNull(streamCursor, "streamCursor");
            Require.NotNull(mutationPipeline, "mutationPipeline");
            Require.NotNull(openCursor, "openCursor");

            m_streamCursor = streamCursor;
            m_mutationPipeline = mutationPipeline;
            m_openCursor = openCursor;
        }

        public async Task ReadEventsAsync()
        {
            ConnectionState.EnsureConnectionIsActive();

            if (m_streamCursor.EndOfStream)
            {
                throw new InvalidOperationException("Stream \"{0}\" is empty.".FormatString(StreamName));
            }

            await m_streamCursor.FetchSlice();

            m_readedEvents = new List<JournaledEvent>(m_streamCursor.Slice.Count);
            foreach (var journaledEvent in m_streamCursor.Slice)
            {
                m_readedEvents.Add(m_mutationPipeline.Mutate(journaledEvent));
            }
        }

        public async Task ContinueAsync()
        {
            ConnectionState.EnsureConnectionIsActive();

            if (IsCompleted)
            {
                m_streamCursor = await m_openCursor(StreamVersion.Increment());
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

        public override StreamVersion StreamVersion
        {
            get { return m_streamCursor.CurrentVersion; }
        }

        public bool HasEvents
        {
            get { return !m_streamCursor.EndOfStream; }
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
