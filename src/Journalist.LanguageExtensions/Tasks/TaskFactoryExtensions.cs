using System.Threading.Tasks;

namespace Journalist.Tasks
{
    public static class TaskFactoryExtensions
    {
        public static Task<T> YieldTask<T>(this T source)
        {
            return Task.FromResult(source);
        }
    }
}