using Journalist.EventSourced.Entities;

namespace Journalist.EventSourced.Application.Repositories
{
    public static class StreamName
    {
        public static string GetForIdentity(IIdentity identity)
        {
            return string.Concat(identity.GetTag(), "-", identity.GetValue());
        }
    }
}