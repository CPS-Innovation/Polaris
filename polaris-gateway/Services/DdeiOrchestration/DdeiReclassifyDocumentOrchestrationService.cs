using Common.Dto.Request;
using Common.Dto.Response;
using Common.Dto.Response.Document;
using Common.Extensions;
using Ddei.Domain.CaseData.Args;
using Ddei.Factories;
using DdeiClient.Enums;
using DdeiClient.Factories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PolarisGateway.Services.DdeiOrchestration;

public class DdeiReclassifyDocumentOrchestrationService : IDdeiReclassifyDocumentOrchestrationService
{
    private readonly IDdeiClientFactory _ddeiClientFactory;
    private readonly IDdeiArgFactory _ddeiArgFactory;

    public DdeiReclassifyDocumentOrchestrationService(
            IDdeiClientFactory ddeiClientFactory,
            IDdeiArgFactory ddeiArgFactory
        )
    {
        _ddeiClientFactory = ddeiClientFactory.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
    }

    public async Task<(bool IsSuccess, DocumentReclassifiedResultDto Result)> ReclassifyDocument(DdeiReclassifyDocumentArgDto arg)
    {
        var mdsClient = _ddeiClientFactory.Create(arg.CmsAuthValues, DdeiClients.Mds);

        var caseDocumentsTask = mdsClient.ListDocumentsAsync(arg);
        var materialTypeListTask = mdsClient.GetMaterialTypeListAsync(arg);

        await Task.WhenAll(caseDocumentsTask, materialTypeListTask);

        var caseDocuments = caseDocumentsTask.Result;
        var materialTypeList = materialTypeListTask.Result;

        var document = caseDocuments.SingleOrDefault(x => x.DocumentId == arg.DocumentId);

        if (document == null)
        {
            return (IsSuccess: false, Result: null);
        }

        var materialType = materialTypeList.SingleOrDefault(x => x.TypeId == arg.DocumentTypeId);

        if (materialType == null)
        {
            return (IsSuccess: false, Result: null);
        }

        var reclassifyCommunicationRequest = new DdeiReclassifyCommunicationArgDto
        {
            CmsAuthValues = arg.CmsAuthValues,
            CorrelationId = arg.CorrelationId,
            Urn = arg.Urn,
            CaseId = arg.CaseId,
            DocumentId = arg.DocumentId,
            Classification = arg.DocumentTypeId == -2 ? "DEFENCESTATEMENT" : materialType.Classification,
            MaterialId = arg.DocumentId,
            DocumentTypeId = arg.DocumentTypeId,
            Subject = document.PresentationTitle,
            Statement = SetReclassifyDocumentStatement(materialType, document, arg),
            Exhibit = SetReclassifyDocumentExhibit(materialType, arg),
            Used = SetReclassifyDocumentUsed(materialType, arg)
        };

        var reclassifyResponse = await mdsClient.ReclassifyCommunicationAsync(reclassifyCommunicationRequest);

        DocumentRenamedResultDto documentRenamedResult = new();
        bool documentRenamed = false;

        if (!string.IsNullOrEmpty(arg.Other?.DocumentName) || !string.IsNullOrEmpty(arg.Immediate?.DocumentName))
        {
            var documentName = !string.IsNullOrEmpty(arg.Other?.DocumentName) ? arg.Other?.DocumentName : arg.Immediate?.DocumentName;
            documentRenamedResult = await RenameDocument(arg, mdsClient, document, documentName);
            documentRenamed = true;
        }

        return (IsSuccess: true, Result: new DocumentReclassifiedResultDto
        {
            DocumentId = reclassifyResponse.ReclassifyCommunication.Id,
            DocumentTypeId = materialType.TypeId,
            OriginalDocumentTypeId = document.CmsDocType.DocumentTypeId ?? 0,
            ReclassificationType = materialType.Classification,
            DocumentRenamed = documentRenamed,
            DocumentRenamedOperationName = documentRenamedResult.OperationName
        });
    }

    private async Task<DocumentRenamedResultDto> RenameDocument(DdeiReclassifyDocumentArgDto arg, DdeiClient.Clients.Interfaces.IDdeiClient mdsClient, CmsDocumentDto document, string documentName)
    {
        var renameDocumentArg = _ddeiArgFactory.CreateRenameDocumentArgDto(arg.CmsAuthValues, arg.CorrelationId, arg.Urn, arg.CaseId, arg.DocumentId, documentName);

        if (document.CmsDocType.DocumentCategory == "EXHIBIT")
        {
            return await mdsClient.RenameExhibitAsync(renameDocumentArg);
        }
        else if (document.CmsDocType.DocumentCategory != "STATEMENT")
        {
            return await mdsClient.RenameDocumentAsync(renameDocumentArg);
        }

        return new DocumentRenamedResultDto();
    }

    private static ReclassificationStatement SetReclassifyDocumentStatement(MaterialTypeDto materialType, CmsDocumentDto document, DdeiReclassifyDocumentArgDto documentReclassify)
    {
        if (materialType.Classification == "STATEMENT")
        {
            var statementDate = DateTime.Parse(documentReclassify.Statement.Date);
            var statementNo = documentReclassify.Statement.StatementNo;

            if (statementNo == 0)
            {
                //var currentStatementNo = int.Parse(document.Title);
                // statementNo = currentStatementNo++;
            }

            return new ReclassificationStatement
            {
                StatementNo = statementNo,
                WitnessId = documentReclassify.Statement.WitnessId,
                Date = statementDate.ToString("yyyy-MM-dd")
            };
        }
        else
            return null;
    }

    private static ReclassificationExhibit SetReclassifyDocumentExhibit(MaterialTypeDto materialType, DdeiReclassifyDocumentArgDto documentReclassify)
    {
        if (materialType.Classification == "EXHIBIT")
        {
            return new ReclassificationExhibit
            {
                Item = documentReclassify.Exhibit.Item,
                Reference = documentReclassify.Exhibit.Reference,
                ExistingProducerOrWitnessId = documentReclassify.Exhibit.ExistingProducerOrWitnessId,
                NewProducer = documentReclassify.Exhibit.NewProducer
            };
        }
        else
            return null;
    }

    private static bool? SetReclassifyDocumentUsed(MaterialTypeDto materialType, DdeiReclassifyDocumentArgDto documentReclassifyArgument)
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