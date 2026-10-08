// <copyright file="GeneratePdfFromDefendantsAndCharges.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Durable.Activity;

using Common.Clients.PdfGenerator;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Services.BlobStorage;
using Common.Services.RenderHtmlService;
using coordinator.Domain;
using coordinator.Durable.Activity.GeneratePdf;
using coordinator.Durable.Payloads;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Clients.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

public class GeneratePdfFromDefendantsAndCharges : BaseGeneratePdf
{
    private readonly IMasterDataServiceClient masterDataServiceClient;
    private readonly ICaseDetailsMapper caseDetailsMapper;
    private readonly IConvertModelToHtmlService convertPcdRequestToHtmlService;

    public GeneratePdfFromDefendantsAndCharges(
        IPdfGeneratorClient pdfGeneratorClient,
        IMdsClient mdsClient,
        IMasterDataServiceClient masterDataServiceClient,
        ICaseDetailsMapper caseDetailsMapper,
        Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
        IMdsArgFactory mdsArgFactory,
        IConvertModelToHtmlService convertPcdRequestToHtmlService,
        IConfiguration configuration)
        : base(mdsArgFactory, blobStorageServiceFactory, pdfGeneratorClient, configuration, mdsClient)
    {
        this.masterDataServiceClient = masterDataServiceClient;
        this.caseDetailsMapper = caseDetailsMapper;
        this.convertPcdRequestToHtmlService = convertPcdRequestToHtmlService;
    }

    [Function(nameof(GeneratePdfFromDefendantsAndCharges))]
    public new async Task<PdfConversionResponse> Run([ActivityTrigger] DocumentPayload payload)
    {
        return await base.Run(payload);
    }

    protected override async Task<Stream> GetDocumentStreamAsync(DocumentPayload payload)
    {
        var arg = this.MdsArgFactory.CreateCaseIdentifiersArg(
                        payload.CmsAuthValues,
                        payload.CorrelationId,
                        payload.Urn,
                        payload.CaseId);

        var defendantsAndCharges = await this.masterDataServiceClient.GetCaseDefendantsAsync(
            new ListCaseDefendantsRequest(arg.CaseId, arg.CorrelationId),
            new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));

        var mappedDefendants = this.caseDetailsMapper.MapDefendantsResponseToDefendantsAndChargesListDto(defendantsAndCharges, arg.CaseId);

        return await this.convertPcdRequestToHtmlService.ConvertAsync(mappedDefendants);
    }
}
