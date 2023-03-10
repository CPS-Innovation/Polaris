using System;
namespace Common.Domain.Exceptions
{
	[Serializable]
	public class OcrServiceException : Exception
	{
		public OcrServiceException(string message) : base(message)
		{
		}
	}
}

