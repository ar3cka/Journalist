using System.Threading.Tasks;
using Journalist.Tasks;

namespace Journalist.EventStore.UnitTests.Infrastructure.Stubs
{
    public class CommitStreamVersionFMock
    {
        public Task Invoke(StreamVersion version)
        {
            CallsCount++;
            CommitedVersion = version;

            return TaskDone.Done;
        }

        public StreamVersion CommitedVersion { get; private set; }

        public int CallsCount { get; private set; }
    }
}
