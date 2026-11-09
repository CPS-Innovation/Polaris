// <copyright file="GeneratePdfFromPcdRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Durable.Activity;

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

public class GeneratePdfFromPcdRequest : BaseGeneratePdf
{
    private readonly IConvertModelToHtmlService convertPcdRequestToHtmlService;
    public GeneratePdfFromPcdRequest(
        IPdfGeneratorClient pdfGeneratorClient,
        IMdsClient mdsClient,
        IMasterDataServiceClient masterDataServiceClient,
        Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
        IMdsArgFactory mdsArgFactory,
        IConvertModelToHtmlService convertPcdRequestToHtmlService,
        IConfiguration configuration)
        : base(mdsArgFactory, blobStorageServiceFactory, pdfGeneratorClient, configuration, mdsClient, masterDataServiceClient)
    {
        this.convertPcdRequestToHtmlService = convertPcdRequestToHtmlService;

    }

    [Function(nameof(GeneratePdfFromPcdRequest))]
    public new async Task<PdfConversionResponse> Run([ActivityTrigger] DocumentPayload payload)
    {
        return await base.Run(payload);
    }

    protected override async Task<Stream> GetDocumentStreamAsync(DocumentPayload payload)
    {
        var arg = this.MdsArgFactory.CreatePcdArg(
            payload.CmsAuthValues,
            payload.CorrelationId,
            payload.Urn,
            payload.CaseId,
            payload.MaterialId);

        var pcdRequest = await this.masterDataServiceClient.GetPcdRequestByPcdIdAsync(
            new GetPcdRequestByPcdIdCoreRequest(arg.CaseId, arg.PcdId, arg.CorrelationId),
            new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));

        return await this.convertPcdRequestToHtmlService.ConvertAsync(pcdRequest);
    }
}
