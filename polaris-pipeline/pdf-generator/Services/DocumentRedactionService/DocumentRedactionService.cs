using System;
using System.IO;
using System.Threading.Tasks;
using Aspose.Pdf;
using Aspose.Pdf.Annotations;
//using Aspose.Pdf.Devices;
using Aspose.Pdf.Facades;
using Common.Domain.Extensions;
using Common.Domain.Requests;
using Common.Domain.Responses;
using Common.Logging;
using Common.Services.BlobStorageService.Contracts;
using Microsoft.Extensions.Logging;

namespace pdf_generator.Services.DocumentRedactionService
{
    public class DocumentRedactionService : IDocumentRedactionService
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly ICoordinateCalculator _coordinateCalculator;
        private readonly ILogger<DocumentRedactionService> _logger;

        public DocumentRedactionService(IBlobStorageService blobStorageService, ICoordinateCalculator coordinateCalculator, ILogger<DocumentRedactionService> logger)
        {
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _coordinateCalculator = coordinateCalculator ?? throw new ArgumentNullException(nameof(coordinateCalculator));
            _logger = logger;
        }

        public async Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequest redactPdfRequest, string accessToken, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(RedactPdfAsync), redactPdfRequest.ToJson());
            var saveResult = new RedactPdfResponse();

            //1. Load PDF from BLOB storage
            _logger.LogMethodFlow(correlationId, nameof(RedactPdfAsync), $"Load '{redactPdfRequest.FileName}' from Blob Storage");
            var fileName = redactPdfRequest.FileName;
            var document = await _blobStorageService.GetDocumentAsync(fileName, correlationId);
            if (document == null)
            {
                saveResult.Succeeded = false;
                saveResult.Message = $"Invalid document - a document with filename '{fileName}' could not be retrieved for redaction purposes";
                return saveResult;
            }

            var fileNameWithoutExtension = fileName.IndexOf(".pdf", StringComparison.OrdinalIgnoreCase) > -1 ? fileName.Split(".pdf", StringSplitOptions.RemoveEmptyEntries)[0] : fileName;

            var newFileName = $"{fileNameWithoutExtension}_{DateTime.Now.Ticks.GetHashCode().ToString("x").ToUpper()}.pdf"; //restore save redaction to same storage for now, but with additional randomised identifier
            //this will be replaced shortly by the TDE upload call

            //2. Apply UI instructions by drawing boxes according to co-ordinate data onto existing PDF
            _logger.LogMethodFlow(correlationId, nameof(RedactPdfAsync), "Apply UI instructions by drawing boxes according to co-ordinate data onto existing PDF");
            using var redactedDocument = new Document(document);
            var pdfInfo = new PdfFileInfo(redactedDocument);

            foreach (var redactionPage in redactPdfRequest.RedactionDefinitions)
            {
                var currentPage = redactionPage.PageIndex;
                var annotationPage = redactedDocument.Pages[currentPage];
                
                foreach (var boxToDraw in redactionPage.RedactionCoordinates)
                {
                    var translatedCoordinates = _coordinateCalculator.CalculateRelativeCoordinates(redactionPage.Width,
                        redactionPage.Height, currentPage, boxToDraw, pdfInfo, correlationId);

                    var annotationRectangle = new Rectangle(translatedCoordinates.X1, translatedCoordinates.Y1, translatedCoordinates.X2, translatedCoordinates.Y2);
                    var redactionAnnotation = new RedactionAnnotation(annotationPage, annotationRectangle)
                    {
                        FillColor = Color.Black
                    };

                    redactedDocument.Pages[currentPage].Annotations.Add(redactionAnnotation, true);
                    redactionAnnotation.Redact();
                }
            }
            
            _logger.LogMethodFlow(correlationId, nameof(RedactPdfAsync), "Remove redacted document metadata");
            redactedDocument.RemoveMetadata();

            //3. Save the flattened PDF into BLOB storage but with a new filename (FOR NOW, until we have a tactical or "for-reals" API solution/integration in place)
            // Check the redacted document version - if less than 1.7 then attempt to convert 
            _logger.LogMethodFlow(correlationId, nameof(RedactPdfAsync), $"Save the flattened PDF into 'local' BLOB storage but with a new filename, for now - new filename: {newFileName}");

            using var redactedDocumentStream = new MemoryStream();
            if (IsCandidateForConversion(redactedDocument.PdfFormat))
            {
                var conversionOptions = new PdfFormatConversionOptions(PdfFormat.v_1_7);
                if (redactedDocument.Validate(conversionOptions))
                {
                    try
                    {
                        using var convertedDocumentSteam = new MemoryStream();
                        redactedDocument.Convert(convertedDocumentSteam, PdfFormat.v_1_7, ConvertErrorAction.Delete);
                        redactedDocument.Save(redactedDocumentStream);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogMethodError(correlationId, nameof(RedactPdfAsync), "Could not convert the PDF document to version 1.7, saving 'as-is' in original format", ex);
                        redactedDocument.Save(redactedDocumentStream);
                    }
                }
                else
                {
                    redactedDocument.Save(redactedDocumentStream);
                }
            }
            else
            {
                redactedDocument.Save(redactedDocumentStream);
            }
            
            await _blobStorageService.UploadDocumentAsync(redactedDocumentStream, newFileName, redactPdfRequest.CaseId.ToString(), redactPdfRequest.DocumentId, redactPdfRequest.VersionId.ToString(), correlationId);

            saveResult.Succeeded = true;
            saveResult.RedactedDocumentName = newFileName;

            _logger.LogMethodExit(correlationId, nameof(RedactPdfAsync), saveResult.ToJson());
            return saveResult;
        }
        
        private static bool IsCandidateForConversion(PdfFormat currentVersion)
        {
            return currentVersion is PdfFormat.v_1_0 or PdfFormat.v_1_1 or PdfFormat.v_1_2 or PdfFormat.v_1_3 or PdfFormat.v_1_4 or PdfFormat.v_1_5 or PdfFormat.v_1_6;
        }
    }
}