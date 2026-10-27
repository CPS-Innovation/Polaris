// <copyright file="DocumentRedactionService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace pdf_redactor.Services.DocumentRedaction;

using Common.Dto.Request;
using Common.Logging;
using Microsoft.Extensions.Logging;

public class DocumentRedactionService(
        IRedactionProvider redactionProvider,
        ILogger<DocumentRedactionService> logger)
    : IDocumentRedactionService
{
    public async Task<Stream> RedactAsync(int caseId, string documentId, RedactPdfRequestWithDocumentDto redactPdfRequest, Guid correlationId)
    {
        try
        {
            byte[] documentBytes = Convert.FromBase64String(redactPdfRequest.Document);
            using var documentStream = new MemoryStream(documentBytes);

            RedactPdfRequestDto pdfRedact = new ()
            {
                RedactionDefinitions = redactPdfRequest.RedactionDefinitions,
            };

            return await redactionProvider.Redact(documentStream, caseId, documentId, pdfRedact, correlationId);
        }
        catch (Exception ex)
        {
            logger.LogMethodError(correlationId, nameof(this.RedactAsync), ex.Message, ex);
            throw;
        }
    }
}
