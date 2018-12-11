namespace Journalist.EventStore.Streams
{
    public enum ReceivingResultCode
    {
        EventsReceived = 0,
        EmptyStream = 1,
        PromotionFailed = 2
    }
}
