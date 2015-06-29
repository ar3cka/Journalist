namespace Journalist.EventStore.Events
{
    public static class JournaledEventPropertyNames
    {
        public static readonly string EventId = "EventId";
        public static readonly string EventType = "EventType";
        public static readonly string EventPayload = "EventPayload";

        public static readonly string[] All =
        {
            EventId,
            EventType,
            EventPayload
        };
    }
}
