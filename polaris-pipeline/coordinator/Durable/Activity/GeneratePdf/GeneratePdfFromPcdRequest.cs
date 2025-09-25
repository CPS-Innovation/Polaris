using Common.Clients.PdfGenerator;
using Common.Services.BlobStorage;
using Common.Services.RenderHtmlService;
using coordinator.Domain;
using coordinator.Durable.Activity.GeneratePdf;
using coordinator.Durable.Payloads;
using DdeiClient.Clients.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using DdeiClient.Factories;

namespace coordinator.Durable.Activity;

public class GeneratePdfFromPcdRequest : BaseGeneratePdf
{
    private readonly IConvertModelToHtmlService _convertPcdRequestToHtmlService;
    public GeneratePdfFromPcdRequest(
        IPdfGeneratorClient pdfGeneratorClient,
        IMdsClient mdsClient,
        Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
        IDdeiArgFactory ddeiArgFactory,
        IConvertModelToHtmlService convertPcdRequestToHtmlService,
        IConfiguration configuration)
        : base(ddeiArgFactory, blobStorageServiceFactory, pdfGeneratorClient, configuration, mdsClient)
    {
        _convertPcdRequestToHtmlService = convertPcdRequestToHtmlService;

    }

    [Function(nameof(GeneratePdfFromPcdRequest))]
    public new async Task<PdfConversionResponse> Run([ActivityTrigger] DocumentPayload payload)
    {
        return await base.Run(payload);
    }

    protected override async Task<Stream> GetDocumentStreamAsync(DocumentPayload payload)
    {
        var arg = DdeiArgFactory.CreatePcdArg(
            payload.CmsAuthValues,
            payload.CorrelationId,
            payload.Urn,
            payload.CaseId,
            payload.DocumentId);
            
        var pcdRequest = await MdsClient.GetPcdRequestAsync(arg);
        return await _convertPcdRequestToHtmlService.ConvertAsync(pcdRequest);
    }
}