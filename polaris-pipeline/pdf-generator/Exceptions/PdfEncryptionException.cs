using System;

namespace pdf_generator.Exceptions;

[Serializable]
public class PdfEncryptionException : Exception
{
    public PdfEncryptionException() :
        base("Pdf is encrypted.")
    {
    }
}