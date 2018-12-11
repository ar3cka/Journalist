using System;
using System.Threading.Tasks;

namespace Journalist.EventStore.Journal.Persistence.Operations
{
    public interface IStreamOperation<TResult>
    {
        Task<TResult> ExecuteAsync();

        void Handle(Exception exception);
    }
}
