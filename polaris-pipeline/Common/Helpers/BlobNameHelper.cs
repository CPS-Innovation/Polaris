namespace Common.Helpers;

// This is temporary code to help us through the current refactor
public static class BlobNameHelper
{
    public static string GetBlobName(int caseId, string cmsOrInternalDocumentId, BlobType blobType)
    {
        // Each case has only one defendants and charges (DAC) document.
        //  If the caseId is then the DocumentId for a DAC is DAC-12345
        //  The PdfBlobName has always been CMS-DAC.pdf.
        //  While we are doing the refactor we keep this, but this whole thing is to be reworked.

        // If we come though here with a DAC, cmsDocumentId will be "DAC" but DocumentId will be e.g. "DAC-12345"
        //  But the PdfBlobName is always DAC.pdf
        if (cmsOrInternalDocumentId.StartsWith("DAC"))
        {
            cmsOrInternalDocumentId = "DAC";
        }

        if (cmsOrInternalDocumentId.StartsWith("CMS-"))
        {
            cmsOrInternalDocumentId = cmsOrInternalDocumentId.Substring(4);
        }

        return blobType switch
        {
            BlobType.Pdf => $"{caseId}/pdfs/CMS-{cmsOrInternalDocumentId}.pdf",
            BlobType.Ocr => $"{caseId}/ocrs/CMS-{cmsOrInternalDocumentId}.json",
            BlobType.Pii => $"{caseId}/pii/CMS-{cmsOrInternalDocumentId}.json",
            _ => throw new System.NotImplementedException()
        };
    }

    public static string GetBlobName(int caseId, string cmsOrInternalDocumentId, long versionId, BlobType blobType)
    {
        // Each case has only one defendants and charges (DAC) document.
        //  If the caseId is then the DocumentId for a DAC is DAC-12345
        //  The PdfBlobName has always been CMS-DAC.pdf.
        //  While we are doing the refactor we keep this, but this whole thing is to be reworked.

        if (long.TryParse(cmsOrInternalDocumentId, out _))
        {
            cmsOrInternalDocumentId = $"CMS-{cmsOrInternalDocumentId}";
        }

        return blobType switch
        {
            BlobType.Pdf => $"{caseId}/pdfs/{cmsOrInternalDocumentId}-{versionId}.pdf",
            BlobType.Ocr => $"{caseId}/ocrs/{cmsOrInternalDocumentId}-{versionId}.json",
            BlobType.Pii => $"{caseId}/pii/{cmsOrInternalDocumentId}-{versionId}.json",
            _ => throw new System.NotImplementedException()
        };
    }


    public enum BlobType
    {
        Pdf,
        Ocr,
        Pii
    }
}