using System;
using Journalist.WindowsAzure.Storage.Internals;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Journalist.WindowsAzure.Storage.Queues
{
    public class CloudQueueAdapter : CloudEntityAdapter<CloudQueue>, ICloudQueue
    {
        public CloudQueueAdapter(Func<CloudQueue> entityFactory) : base(entityFactory)
        {
        }
    }
}
