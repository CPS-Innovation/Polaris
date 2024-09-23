using Common.Constants;
using Common.Dto.Request;
using Common.Dto.Request.DocumentManipulation;

namespace pdf_redactor.integration.tests.Helpers
{
    public class DocumentManipulationHelper
    {
        public static ModifyDocumentWithDocumentDto LoadDocumentModificationDataForPdf(Stream pdfStream, string fileName, int[]? pagesToRemove, Dictionary<int, string>? pagesToRotate)
        {
            using var documentReader = new BinaryReader(pdfStream);
            var documentBytes = documentReader.ReadBytes((int)pdfStream.Length);
            var base64Document = Convert.ToBase64String(documentBytes);

            var documentModifications = new List<DocumentModificationDto>();

            if (pagesToRemove != null)
                foreach (var pageIndex in pagesToRemove)
                    documentModifications.Add(new DocumentModificationDto { PageIndex = pageIndex, Operation = DocumentManipulationOperation.RemovePage });

            if (pagesToRotate != null)
                foreach (var page in pagesToRotate)
                    documentModifications.Add(new DocumentModificationDto { PageIndex = page.Key, Arg = page.Value, Operation = DocumentManipulationOperation.RotatePage });

            return new ModifyDocumentWithDocumentDto
            {
                FileName = fileName,
                DocumentModifications = documentModifications,
                Document = base64Document,
                VersionId = 1
            };
        }
    }
}