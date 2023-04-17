using System;
namespace Common.Domain.Exceptions
{
	[Serializable]
	public class UnsupportedFileTypeException : Exception
	{
		public UnsupportedFileTypeException(string value) :
			base($"File type '{value}' not supported.")
		{
		}
	}
}

