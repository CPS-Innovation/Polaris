using System;

namespace Common.Helpers;

// This is temporary code to help us through the current refactor
public static class BlobNameHelper
{
    public static string GetBlobName(int caseId, string documentId, long versionId, BlobType blobType)
    {
        // Each case has only one defendants and charges (DAC) document.
        //  If the caseId is then the DocumentId for a DAC is DAC-12345
        //  The PdfBlobName has always been CMS-DAC.pdf.
        //  While we are doing the refactor we keep this, but this whole thing is to be reworked.

        if (long.TryParse(documentId, out _))
        {
            throw new ArgumentOutOfRangeException(nameof(documentId), "DocumentId should not be a number");
            //documentId = $"CMS-{documentId}";
        }

        return blobType switch
        {
            BlobType.Pdf => $"{caseId}/pdfs/{documentId}-{versionId}.pdf",
            BlobType.Ocr => $"{caseId}/ocrs/{documentId}-{versionId}.json",
            BlobType.Pii => $"{caseId}/pii/{documentId}-{versionId}.json",
            _ => throw new NotImplementedException()
        };
    }

    public static string GetSafePrefix(int caseId)
    {
        return $"{caseId}/";
    }

    public enum BlobType
    {
        Pdf,
        Ocr,
        Pii
    }
}