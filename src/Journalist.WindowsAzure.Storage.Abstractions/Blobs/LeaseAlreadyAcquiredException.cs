using System;
using System.Runtime.Serialization;

namespace Journalist.WindowsAzure.Storage.Blobs
{
	[Serializable]
	public sealed class LeaseAlreadyAcquiredException : Exception
	{
		public LeaseAlreadyAcquiredException()
		{
		}

		public LeaseAlreadyAcquiredException(string message) : base(message)
		{
		}

		public LeaseAlreadyAcquiredException(string message, Exception inner) : base(message, inner)
		{
		}

		private LeaseAlreadyAcquiredException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
