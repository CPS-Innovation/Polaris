using System;
namespace pdf_generator.Domain.Exceptions
{
	[Serializable]
	public class PdfConversionException : Exception
	{
		public PdfConversionException(string documentId, string message) :
			base($"Unable to convert document with id '{documentId}' to pdf. Exception: {message}.")
		{
		}
	}
}

