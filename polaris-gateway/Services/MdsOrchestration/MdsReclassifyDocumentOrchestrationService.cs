// <copyright file="MdsReclassifyDocumentOrchestrationService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Services.MdsOrchestration;

using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response;
using Common.Dto.Response.Document;
using Common.Dto.Response.HouseKeeping;
using Common.Extensions;
using Common.Mappers;
using Cps.MasterDataService.Infrastructure.ApiClient;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.Response.Document;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

public class MdsReclassifyDocumentOrchestrationService : IMdsReclassifyDocumentOrchestrationService
{
    private const string ExhibitClassification = "EXHIBIT";
    private const string StatementClassification = "STATEMENT";
    private const string DefenceStatementClassification = "DEFENCESTATEMENT";
    private const int DefenceStatementTypeId = -2;

    private readonly IMdsClient mdsClient;
    private readonly IMasterDataServiceClient masterDataServiceClient;
    private readonly ICmsDocumentDtoMapper cmsDocumentDtoMapper;
    private readonly IMdsArgFactory mdsArgFactory;

    public MdsReclassifyDocumentOrchestrationService(
            IMdsClient mdsClient,
            IMasterDataServiceClient masterDataServiceClient,
            ICmsDocumentDtoMapper cmsDocumentDtoMapper,
            IMdsArgFactory mdsArgFactory
        )
    {
        this.mdsClient = mdsClient.ExceptionIfNull();
        this.masterDataServiceClient = masterDataServiceClient.ExceptionIfNull();
        this.cmsDocumentDtoMapper = cmsDocumentDtoMapper.ExceptionIfNull();
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
    }

    public async Task<DocumentReclassifiedResult> ReclassifyDocument(MdsReclassifyDocumentArgDto arg)
    {
        var (caseDocuments, materialTypeList) = await this.FetchDocumentAndMaterialTypes(this.mdsClient, arg);

        var document = caseDocuments.SingleOrDefault(x => x.DocumentId == arg.MaterialId);
        if (document == null)
        {
            return new DocumentReclassifiedResult { IsSuccess = false, Result = null };
        }

        var materialType = materialTypeList.SingleOrDefault(x => x.TypeId == arg.DocumentTypeId);
        if (materialType == null)
        {
            return new DocumentReclassifiedResult { IsSuccess = false, Result = null };
        }

        var reclassifyResponse = await this.ReclassifyDocument(this.mdsClient, arg, document, materialType);

        var (documentRenamed, documentRenamedResult) = await this.HandleDocumentRenaming(arg, this.mdsClient, materialType);

        return new DocumentReclassifiedResult
        {
            IsSuccess = true,
            Result = new DocumentReclassifiedResultDto
            {
                DocumentId = reclassifyResponse.ReclassifyCommunication.Id,
                DocumentTypeId = materialType.TypeId,
                OriginalDocumentTypeId = document.CmsDocType.DocumentTypeId ?? 0,
                ReclassificationType = materialType.Classification,
                DocumentRenamed = documentRenamed,
                DocumentRenamedOperationName = documentRenamedResult.OperationName,
            },
        };
    }

    private async Task<(IEnumerable<CmsDocumentDto> caseDocuments, IEnumerable<MaterialTypeDto> materialTypeList)> FetchDocumentAndMaterialTypes(IMdsClient mdsClient, MdsReclassifyDocumentArgDto arg)
    {
        var caseDocumentsTask = this.masterDataServiceClient.ListDocumentsAsync(arg, new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));
        var materialTypeListTask = mdsClient.GetMaterialTypeListAsync(arg);

        await Task.WhenAll(caseDocumentsTask, materialTypeListTask);

        var caseDocumentsMapped = caseDocumentsTask.Result.Select(x => this.cmsDocumentDtoMapper.Map(x, null)).ToList();

        return (caseDocumentsMapped, materialTypeListTask.Result);
    }

    private async Task<MdsCommunicationReclassifiedResponse> ReclassifyDocument(IMdsClient mdsClient, MdsReclassifyDocumentArgDto arg, CmsDocumentDto document, MaterialTypeDto materialType)
    {
        var statement = SetReclassifyDocumentStatement(materialType, document, arg);
        ReclassifyStatementRequest statementRequest = null;
        if (statement != null)
        {
            statementRequest = new ReclassifyStatementRequest
            {
                Witness = statement.WitnessId,
                StatementNo = statement.StatementNo,
                Date = DateOnly.ParseExact(
                    statement.Date,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture),
            };
        }

        var exhibit = SetReclassifyDocumentExhibit(materialType, arg);
        ReclassifyExhibitRequest exhibitRequest = null;
        if (exhibit != null)
        {
            exhibitRequest = new ReclassifyExhibitRequest
            {
                Item = exhibit.Item,
                Reference = exhibit.Reference,
                ExistingProducerOrWitnessId = exhibit.ExistingProducerOrWitnessId,
                NewProducer = exhibit.NewProducer,
            };
        }

        var request = new ReclassifyCommunicationRequest(
            id: arg.CorrelationId,
            classification: arg.DocumentTypeId == DefenceStatementTypeId ? DefenceStatementClassification : materialType.Classification,
            materialId: (int)arg.MaterialId,
            documentTypeId: arg.DocumentTypeId,
            used: SetReclassifyDocumentUsed(materialType, arg) ?? false,
            subject: document.PresentationTitle,
            statementRequest: statementRequest,
            exhibitRequest: exhibitRequest);

        var reclassificationRespponse = await this.masterDataServiceClient.ReclassifyCommunicationAsync(request, new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));

        return new MdsCommunicationReclassifiedResponse
        {
            ReclassifyCommunication = new Ddei.Domain.Response.Document.ReclassifyCommunication
            {
                Id = reclassificationRespponse.ReclassifyCommunication.Id,
            },
        };
    }

    private async Task<DocumentRenamedResultDto> RenameDocument(MdsReclassifyDocumentArgDto arg, IMdsClient mdsClient, MaterialTypeDto materialType, string documentName) 
    {
        var renameDocumentArg = this.mdsArgFactory.CreateRenameDocumentArgDto(arg.CmsAuthValues, arg.CorrelationId, arg.Urn, arg.CaseId, arg.MaterialId, documentName);
        Common.Dto.Response.HouseKeeping.RenameMaterialResponse renameMaterialResponse;
        DocumentRenamedResultDto response = new ();

        if (materialType.Classification == ExhibitClassification)
        {
            response = await mdsClient.RenameExhibitAsync(renameDocumentArg);
            response.OperationName = nameof(mdsClient.RenameExhibitAsync);
        }
        else if (materialType.Classification != StatementClassification)
        {
            //response = await mdsClient.RenameDocumentAsync(renameDocumentArg);
            //response.OperationName = nameof(mdsClient.RenameDocumentAsync);

            renameMaterialResponse = await this.masterDataServiceClient.RenameMaterialAsync(
                    new Common.Dto.Request.HouseKeeping.RenameMaterialRequest(renameDocumentArg.CorrelationId, (int)renameDocumentArg.MaterialId, renameDocumentArg.DocumentName),
                    new CmsAuthValues(renameDocumentArg.CmsAuthValues, renameDocumentArg.CorrelationId));

            response.Id = renameMaterialResponse.RenameMaterialData?.Id ?? 0;
            response.OperationName = nameof(this.masterDataServiceClient.RenameMaterialAsync);

        }

        return response;
    }

    private async Task<(bool documentRenamed, DocumentRenamedResultDto documentRenamedResult)> HandleDocumentRenaming(MdsReclassifyDocumentArgDto arg, IMdsClient mdsClient, MaterialTypeDto materialType)
    {
        if (string.IsNullOrEmpty(arg.Other?.DocumentName) && string.IsNullOrEmpty(arg.Immediate?.DocumentName))
        {
            return (false, new DocumentRenamedResultDto());
        }

        var documentName = !string.IsNullOrEmpty(arg.Other?.DocumentName) ? arg.Other?.DocumentName : arg.Immediate?.DocumentName;
        var documentRenamedResult = await this.RenameDocument(arg, mdsClient, materialType, documentName);

        return (true, documentRenamedResult);
    }

    private static ReclassificationStatement SetReclassifyDocumentStatement(MaterialTypeDto materialType, CmsDocumentDto document, MdsReclassifyDocumentArgDto documentReclassify)
    {
        if (materialType.Classification == StatementClassification)
        {
            var statementDate = DateTime.Parse(documentReclassify.Statement.Date);
            var statementNo = documentReclassify.Statement.StatementNo;

            if (statementNo == 0)
            {
                var currentStatementNo = int.Parse(document.Title);
                statementNo = currentStatementNo++;
            }

            return new ReclassificationStatement
            {
                StatementNo = statementNo,
                WitnessId = documentReclassify.Statement.WitnessId,
                Date = statementDate.ToString("yyyy-MM-dd"),
            };
        }

        return null;
    }

    private static ReclassificationExhibit SetReclassifyDocumentExhibit(MaterialTypeDto materialType, MdsReclassifyDocumentArgDto documentReclassify)
    {
        if (materialType.Classification == ExhibitClassification)
        {
            return new ReclassificationExhibit
            {
                Item = documentReclassify.Exhibit.Item,
                Reference = documentReclassify.Exhibit.Reference,
                ExistingProducerOrWitnessId = documentReclassify.Exhibit.ExistingProducerOrWitnessId,
                NewProducer = documentReclassify.Exhibit.NewProducer,
            };
        }

        return null;
    }

    private static bool? SetReclassifyDocumentUsed(MaterialTypeDto materialType, MdsReclassifyDocumentArgDto documentReclassifyArgument)
    {
        if (materialType.AddAsUsedOrUnused == "N" || materialType.AddAsUsedOrUnused == null)
        {
            return null;
        }

        return documentReclassifyArgument switch
        {
            { Exhibit: not null } => documentReclassifyArgument.Exhibit.Used,
            { Statement: not null } => documentReclassifyArgument.Statement.Used,
            { Other: not null } => documentReclassifyArgument.Other.Used,
            _ => null
        };
    }
}
