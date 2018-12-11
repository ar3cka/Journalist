using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Journal;
using Journalist.Options;

namespace Journalist.EventStore.Streams
{
    public class ConsumersService : IConsumersService
    {
        private readonly IEventStreamConsumers m_eventStreamConsumers;
        private readonly IEventJournal m_eventJournal;

        public ConsumersService(IEventStreamConsumers eventStreamConsumers, IEventJournal eventJournal)
        {
            Require.NotNull(eventStreamConsumers, nameof(eventStreamConsumers));
            Require.NotNull(eventJournal, nameof(eventJournal));

            m_eventStreamConsumers = eventStreamConsumers;
            m_eventJournal = eventJournal;
        }

        public async Task<IEnumerable<ConsumerDescription>> EnumerateConsumersAsync(string streamName)
        {
            Require.NotEmpty(streamName, nameof(streamName));

            var streamReaderDescriptions = await m_eventJournal.GetStreamReadersDescriptionsAsync(streamName);

            var descriptionsWithConsumerNames = streamReaderDescriptions.Select(
                streamReaderDescription => new
                {
                    streamReaderDescription,
                    consumerNameTask = m_eventStreamConsumers.TryGetNameAsync(streamReaderDescription.StreamReaderId)
                });

            await Task.WhenAll(descriptionsWithConsumerNames.Select(description => description.consumerNameTask));

            var consumerDescriptions = descriptionsWithConsumerNames
                .Where(description => description.consumerNameTask.Result.IsSome)
                .Select(description => new ConsumerDescription(
                    description.streamReaderDescription.StreamVersion,
                    description.consumerNameTask.Result.GetOrDefault(default(string)),
                    description.streamReaderDescription.StreamReaderId));

            return consumerDescriptions;
        }
    }
}