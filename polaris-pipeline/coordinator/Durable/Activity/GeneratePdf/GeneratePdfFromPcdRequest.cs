using Common.Clients.PdfGenerator;
using Common.Services.BlobStorage;
using Common.Services.RenderHtmlService;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Request;
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

public class GeneratePdfFromPcdRequest : BaseGeneratePdf
{
    private readonly IConvertModelToHtmlService _convertPcdRequestToHtmlService;
    private readonly IMasterDataServiceClient _masterDataServiceClient;
    public GeneratePdfFromPcdRequest(
        IPdfGeneratorClient pdfGeneratorClient,
        IMdsClient mdsClient,
        IMasterDataServiceClient masterDataServiceClient,
        Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
        IMdsArgFactory mdsArgFactory,
        IConvertModelToHtmlService convertPcdRequestToHtmlService,
        IConfiguration configuration)
        : base(mdsArgFactory, blobStorageServiceFactory, pdfGeneratorClient, configuration, mdsClient)
    {
        _convertPcdRequestToHtmlService = convertPcdRequestToHtmlService;
        _masterDataServiceClient = masterDataServiceClient;

    }

    [Function(nameof(GeneratePdfFromPcdRequest))]
    public new async Task<PdfConversionResponse> Run([ActivityTrigger] DocumentPayload payload)
    {
        return await base.Run(payload);
    }

    protected override async Task<Stream> GetDocumentStreamAsync(DocumentPayload payload)
    {
        var arg = MdsArgFactory.CreatePcdArg(
            payload.CmsAuthValues,
            payload.CorrelationId,
            payload.Urn,
            payload.CaseId,
            payload.MaterialId);

        var pcdRequest = await _masterDataServiceClient.GetPcdRequestByPcdIdAsync(
            new GetPcdRequestByPcdIdCoreRequest(arg.CaseId, arg.PcdId, arg.CorrelationId),
            new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));

        return await _convertPcdRequestToHtmlService.ConvertAsync(pcdRequest);
    }
}
