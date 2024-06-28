using Common.Dto.Request;

namespace pdf_redactor.integration.tests.Helpers
{
    public class DocumentManipulationHelper
    {
        public static RemoveDocumentPagesWithDocumentDto LoadPageRemovalDataForPdf(Stream pdfStream, string fileName, int[] pagesIndexesToRemove)
        {
            using var documentReader = new BinaryReader(pdfStream);
            var documentBytes = documentReader.ReadBytes((int)pdfStream.Length);
            var base64Document = Convert.ToBase64String(documentBytes);

            return new RemoveDocumentPagesWithDocumentDto
            {
                FileName = fileName,
                PagesIndexesToRemove = pagesIndexesToRemove,
                Document = base64Document,
                VersionId = "1234"
            };
        }
    }
}