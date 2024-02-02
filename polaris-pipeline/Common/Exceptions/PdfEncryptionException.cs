using System;

namespace Common.Exceptions
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

