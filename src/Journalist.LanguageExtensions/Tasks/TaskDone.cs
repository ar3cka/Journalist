using System.Threading.Tasks;

namespace Journalist.Tasks
{
    public class TaskDone
    {
        public static readonly Task Done = Create();
        public static readonly Task<bool> True = Task.FromResult(true);
        public static readonly Task<bool> False = Task.FromResult(false);

        private static Task Create()
        {
            var completionSource = new TaskCompletionSource<object>();
            completionSource.SetResult(null);

            return completionSource.Task;
        }
    }
}