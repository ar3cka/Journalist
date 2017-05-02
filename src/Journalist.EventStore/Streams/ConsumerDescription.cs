using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;

namespace Journalist.EventStore.Streams
{
	public class ConsumerDescription
	{
		public ConsumerDescription(StreamVersion streamVersion, string consumerName, EventStreamReaderId consumerId)
		{
			Require.NotNull(streamVersion, nameof(streamVersion));
			Require.NotEmpty(consumerName, nameof(consumerName));
			Require.NotNull(consumerId, nameof(consumerId));

			StreamVersion = streamVersion;
			ConsumerName = consumerName;
			ConsumerId = consumerId;
		}

		public StreamVersion StreamVersion { get; }

		public string ConsumerName { get; }

		public EventStreamReaderId ConsumerId { get; }
	}
}