using System.Collections.Generic;
using System.Threading.Tasks;

namespace Journalist.EventStore.Streams
{
	public interface IConsumersService
	{
		Task<IEnumerable<ConsumerDescription>> EnumerateConsumersAsync(string streamName);
	}
}