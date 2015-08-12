using System.Threading.Tasks;

namespace Journalist.EventStore.Journal
{
    public interface IEventJournalReaders
    {
        Task RegisterAsync(EventStreamReaderId readerId);

        Task<bool> IsRegisteredAsync(EventStreamReaderId readerId);
    }
}
