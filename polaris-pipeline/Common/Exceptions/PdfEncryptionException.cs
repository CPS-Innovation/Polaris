using System;
using System.Runtime.Serialization;

namespace Common.Exceptions
{
	[Serializable]
	public class PdfEncryptionException : Exception
	{
		public PdfEncryptionException() :
			base("Pdf is encrypted.")
		{
		}
		
		protected PdfEncryptionException(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{

		}
	}
}

