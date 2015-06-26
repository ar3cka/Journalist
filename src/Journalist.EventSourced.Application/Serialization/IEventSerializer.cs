using Journalist.EventStore.Events;

namespace Journalist.EventSourced.Application.Serialization
{
    public interface IEventSerializer
    {
        object Deserialize(JournaledEvent journaledEvent);

        JournaledEvent Serialize(object change);
    }
}
