namespace Journalist.EventStore.Notifications.Types
{
	public class EventProcessingResult
	{
		public EventProcessingResult(bool isSuccessful, bool shouldCommitProcessing)
		{
			IsSuccessful = isSuccessful;
			ShouldCommitProcessing = shouldCommitProcessing;
		}

		public bool IsSuccessful { get; private set; }

		public bool ShouldCommitProcessing { get; private set; }
	}
}