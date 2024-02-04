namespace coordinator.Domain;

// This is temporary code to help us through the current refactor
public static class PdfBlobNameHelper
{
    public static string GetPdfBlobName(long caseId, string cmsOrPolarisDocumentId)
    {
        return GetPdfBlobName(caseId.ToString(), cmsOrPolarisDocumentId);
    }

    public static string GetPdfBlobName(string caseId, string cmsOrPolarisDocumentId)
    {
        // Each case has only one defendants and charges (DAC) document.
        //  If the caseId is then the PolarisDocumentId for a DAC is DAC-12345
        //  The PdfBlobName has always been CMS-DAC.pdf.
        //  While we are doing the refactor we keep this, but this whole thing is to be reworked.

        // If we come though here with a DAC, cmsDocumentId will be "DAC" but PolarisDocumentId will be e.g. "DAC-12345"
        //  But the PdfBlobName is always DAC.pdf
        if (cmsOrPolarisDocumentId.StartsWith("DAC"))
        {
            cmsOrPolarisDocumentId = "DAC";
        }
        return $"{caseId}/pdfs/CMS-{cmsOrPolarisDocumentId}.pdf";
    }
}
