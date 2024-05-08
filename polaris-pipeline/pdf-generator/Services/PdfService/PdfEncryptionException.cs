using System;

namespace pdf_generator.Services.PdfService
{
	[Serializable]
	public class PdfEncryptionException : Exception
	{
		public PdfEncryptionException() :
			base("Pdf is encrypted.")
		{
		}
	}
}

