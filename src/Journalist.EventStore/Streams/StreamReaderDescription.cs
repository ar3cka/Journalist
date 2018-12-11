using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;

namespace Journalist.EventStore.Streams
{
	public class StreamReaderDescription
	{
		public StreamReaderDescription(string streamName, EventStreamReaderId streamReaderId, StreamVersion streamVersion)
		{
			Require.NotEmpty(streamName, nameof(streamName));
			Require.NotNull(streamReaderId, nameof(streamReaderId));
			Require.NotNull(streamVersion, nameof(streamVersion));

			StreamName = streamName;
			StreamReaderId = streamReaderId;
			StreamVersion = streamVersion;
		}

		public string StreamName { get; }

		public EventStreamReaderId StreamReaderId { get; }

		public StreamVersion StreamVersion { get; }
	}
}