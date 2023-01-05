using System;
namespace pdf_generator.Domain.Exceptions
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

