using System;

namespace coordinator.Services.OcrService
{
	[Serializable]
	public class OcrServiceException : Exception
	{
		public OcrServiceException(string message) : base(message)
		{
		}
	}
}

