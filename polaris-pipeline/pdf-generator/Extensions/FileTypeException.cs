using System;
namespace pdf_generator.Domain.Extensions
{
	[Serializable]
	public class FileTypeException : Exception
	{
		public FileTypeException(string value) :
			base($"File type '{value}' not supported.")
		{
		}
	}
}

