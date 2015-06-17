namespace Journalist.WindowsAzure.Storage.Queues
{
    public class CloudQueueMessageAdapter : ICloudQueueMessage
    {
        private readonly string m_messageId;
        private readonly string m_popReceipt;
        private readonly int m_dequeueCount;
        private readonly byte[] m_content;

        public CloudQueueMessageAdapter(string messageId, string popReceipt, int dequeueCount, byte[] content)
        {
            Require.NotEmpty(messageId, "messageId");
            Require.NotEmpty(popReceipt, "popReceipt");
            Require.ZeroOrGreater(dequeueCount, "dequeueCount");
            Require.NotNull(content, "content");

            m_messageId = messageId;
            m_popReceipt = popReceipt;
            m_dequeueCount = dequeueCount;
            m_content = content;
        }

        public byte[] Content
        {
            get { return m_content; }
        }

        public int DequeueCount
        {
            get { return m_dequeueCount; }
        }

        public string PopReceipt
        {
            get { return m_popReceipt; }
        }

        public string MessageId
        {
            get { return m_messageId; }
        }
    }
}
