using Common.Clients.PdfGenerator;
using Common.Services.BlobStorage;
using coordinator.Domain;
using coordinator.Durable.Activity.GeneratePdf;
using coordinator.Durable.Payloads;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace coordinator.Durable.Activity;

public class GeneratePdfFromDocument : BaseGeneratePdf
{
    public GeneratePdfFromDocument(
        IPdfGeneratorClient pdfGeneratorClient,
        IMdsArgFactory mdsArgFactory,
        Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
        IConfiguration configuration,
        IMdsClient mdsClient)
        : base(mdsArgFactory, blobStorageServiceFactory, pdfGeneratorClient, configuration, mdsClient) { }

    [Function(nameof(GeneratePdfFromDocument))]
    public new async Task<PdfConversionResponse> Run([ActivityTrigger] DocumentPayload payload)
    {
        return await base.Run(payload);
    }

    protected override async Task<Stream> GetDocumentStreamAsync(DocumentPayload payload)
    {
        var arg = MdsArgFactory.CreateDocumentVersionArgDto(
            payload.CmsAuthValues,
            payload.CorrelationId,
            payload.Urn,
            payload.CaseId,
            payload.DocumentId,
            payload.VersionId);

        var result = await MdsClient.GetDocumentAsync(arg);

        return result.Stream;
    }
}