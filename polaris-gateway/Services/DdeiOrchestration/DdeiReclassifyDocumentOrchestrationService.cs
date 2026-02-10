using Common.Dto.Request;
using Common.Dto.Response;
using Common.Dto.Response.Document;
using Common.Extensions;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.Response.Document;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolarisGateway.Services.DdeiOrchestration;

public class DdeiReclassifyDocumentOrchestrationService : IDdeiReclassifyDocumentOrchestrationService
{
    private const string ExhibitClassification = "EXHIBIT";
    private const string StatementClassification = "STATEMENT";
    private const string DefenceStatementClassification = "DEFENCESTATEMENT";
    private const int DefenceStatementTypeId = -2;
    private readonly IMdsClient _mdsClient;
    private readonly IMdsArgFactory _mdsArgFactory;

    public DdeiReclassifyDocumentOrchestrationService(
            IMdsClient mdsClient,
            IMdsArgFactory mdsArgFactory
        )
    {
        _mdsClient = mdsClient.ExceptionIfNull();
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
    }

    public async Task<DocumentReclassifiedResult> ReclassifyDocument(MdsReclassifyDocumentArgDto arg)
    {
        var (caseDocuments, materialTypeList) = await FetchDocumentAndMaterialTypes(_mdsClient, arg);

        var document = caseDocuments.SingleOrDefault(x => x.DocumentId == arg.DocumentId);
        if (document == null) return new DocumentReclassifiedResult { IsSuccess = false, Result = null };

        var materialType = materialTypeList.SingleOrDefault(x => x.TypeId == arg.DocumentTypeId);
        if (materialType == null) return new DocumentReclassifiedResult { IsSuccess = false, Result = null };

        var reclassifyResponse = await ReclassifyDocument(_mdsClient, arg, document, materialType);

        var (documentRenamed, documentRenamedResult) = await HandleDocumentRenaming(arg, _mdsClient, materialType);

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
        var caseDocumentsTask = mdsClient.ListDocumentsAsync(arg);
        var materialTypeListTask = mdsClient.GetMaterialTypeListAsync(arg);

        await Task.WhenAll(caseDocumentsTask, materialTypeListTask);

        return (caseDocumentsTask.Result, materialTypeListTask.Result);
    }

    private async Task<MdsCommunicationReclassifiedResponse> ReclassifyDocument(IMdsClient mdsClient, MdsReclassifyDocumentArgDto arg, CmsDocumentDto document, MaterialTypeDto materialType)
    {
        var reclassifyCommunicationRequest = new MdsReclassifyCommunicationArgDto
        {
            CmsAuthValues = arg.CmsAuthValues,
            CorrelationId = arg.CorrelationId,
            Urn = arg.Urn,
            CaseId = arg.CaseId,
            DocumentId = arg.DocumentId,
            Classification = arg.DocumentTypeId == DefenceStatementTypeId ? DefenceStatementClassification : materialType.Classification,
            MaterialId = arg.DocumentId,
            DocumentTypeId = arg.DocumentTypeId,
            Subject = document.PresentationTitle,
            Statement = SetReclassifyDocumentStatement(materialType, document, arg),
            Exhibit = SetReclassifyDocumentExhibit(materialType, arg),
            Used = SetReclassifyDocumentUsed(materialType, arg),
        };

        return await mdsClient.ReclassifyCommunicationAsync(reclassifyCommunicationRequest);
    }

    private async Task<DocumentRenamedResultDto> RenameDocument(MdsReclassifyDocumentArgDto arg, IMdsClient mdsClient, MaterialTypeDto materialType, string documentName)
    {
        var renameDocumentArg = _mdsArgFactory.CreateRenameDocumentArgDto(arg.CmsAuthValues, arg.CorrelationId, arg.Urn, arg.CaseId, arg.DocumentId, documentName);
        DocumentRenamedResultDto response = new();

        if (materialType.Classification == ExhibitClassification)
        {
            response = await mdsClient.RenameExhibitAsync(renameDocumentArg);
            response.OperationName = nameof(mdsClient.RenameExhibitAsync);
        }
        else if (materialType.Classification != StatementClassification)
        {
            response = await mdsClient.RenameDocumentAsync(renameDocumentArg);
            response.OperationName = nameof(mdsClient.RenameDocumentAsync);
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
        var documentRenamedResult = await RenameDocument(arg, mdsClient, materialType, documentName);

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
                Date = statementDate.ToString("yyyy-MM-dd")
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
        if (materialType.AddAsUsedOrUnused == "N" || materialType.AddAsUsedOrUnused == null) return null;

        return documentReclassifyArgument switch
        {
            { Exhibit: not null } => documentReclassifyArgument.Exhibit.Used,
            { Statement: not null } => documentReclassifyArgument.Statement.Used,
            { Other: not null } => documentReclassifyArgument.Other.Used,
            _ => null
        };
    }
}