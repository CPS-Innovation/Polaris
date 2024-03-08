using System;
namespace text_extractor.Services.OcrService
{
	[Serializable]
	public class OcrServiceException : Exception
	{
		public OcrServiceException(string message) : base(message)
		{
		}
	}
}

