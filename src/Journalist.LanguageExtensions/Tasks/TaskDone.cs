using System.Threading.Tasks;

namespace Journalist.Tasks
{
    public class TaskDone
    {
        public static readonly Task Done = Task.FromResult<object>(null);
        public static readonly Task<bool> True = Task.FromResult(true);
        public static readonly Task<bool> False = Task.FromResult(false);
    }
}