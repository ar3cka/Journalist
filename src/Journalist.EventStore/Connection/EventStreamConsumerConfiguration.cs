using Journalist.Extensions;

namespace Journalist.EventStore.Connection
{
    public class EventStreamConsumerConfiguration : IEventStreamConsumerConfiguration
    {
        private string m_consumerName;
        private string m_streamName;
        private bool m_useAutoCommitProcessedStreamPositionBehavior;

        public EventStreamConsumerConfiguration()
        {
            m_useAutoCommitProcessedStreamPositionBehavior = true;
            m_consumerName = Constants.DEFAULT_STREAM_READER_NAME;
        }

        public IEventStreamConsumerConfiguration ReadStream(string streamName)
        {
            Require.NotEmpty(streamName, "streamName");

            m_streamName = streamName;

            return this;
        }

        public IEventStreamConsumerConfiguration WithName(string consumerName)
        {
            Require.NotEmpty(consumerName, "consumerName");

            m_consumerName = consumerName;

            return this;
        }

        public IEventStreamConsumerConfiguration AutoCommitProcessedStreamPosition(bool autoCommit)
        {
            m_useAutoCommitProcessedStreamPositionBehavior = autoCommit;

            return this;
        }

        public void AssertConfigurationCompleted()
        {
            Ensure.True(m_streamName.IsNotNullOrEmpty(), "Stream name was not specified.");
            Ensure.True(m_consumerName.IsNotNullOrEmpty(), "Consumer name was not specified.");
        }

        public string StreamName
        {
            get { return m_streamName; }
        }

        public bool UseAutoCommitProcessedStreamPositionBehavior
        {
            get { return m_useAutoCommitProcessedStreamPositionBehavior; }
        }

        public string ConsumerName
        {
            get
            {
                Ensure.True(m_consumerName.IsNotNullOrEmpty(), "Consumer name was not specified");

                return m_consumerName;
            }
        }
    }
}
