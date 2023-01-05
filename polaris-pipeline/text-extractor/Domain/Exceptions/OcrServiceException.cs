using System;
namespace text_extractor.Domain.Exceptions
{
	[Serializable]
	public class OcrServiceException : Exception
	{
		public OcrServiceException(string message) : base(message)
		{
		}
	}
}

