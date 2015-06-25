using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Utils;

namespace Journalist.EventStore.Streams
{
    public class EventStreamProducer : IEventStreamProducer
    {
        private readonly IEventStreamWriter m_streamWriter;
        private readonly IRetryPolicy m_retryPolicy;

        public EventStreamProducer(IEventStreamWriter streamWriter, IRetryPolicy retryPolicy)
        {
            Require.NotNull(streamWriter, "streamWriter");
            Require.NotNull(retryPolicy, "retryPolicy");

            m_streamWriter = streamWriter;
            m_retryPolicy = retryPolicy;
        }

        public async Task PublishAsync(IReadOnlyCollection<JournaledEvent> events)
        {
            Require.NotNull(events, "events");

            var tryNumber = 1;
            Exception callException;
            do
            {
                try
                {
                    await m_streamWriter.AppendEventsAsync(events);
                    callException = null;
                }
                catch (EventStreamConcurrencyException exception)
                {
                    callException = exception;
                }

                if (callException != null)
                {
                    tryNumber++;
                    await m_streamWriter.MoveToEndOfStreamAsync();
                }
            }
            while (m_retryPolicy.AllowCall(tryNumber) && callException != null);

            if (callException != null)
            {
                throw callException;
            }
        }
    }
}
