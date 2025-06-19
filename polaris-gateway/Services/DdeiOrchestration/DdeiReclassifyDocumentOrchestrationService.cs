using Common.Dto.Request;
using Common.Dto.Response;
using Common.Dto.Response.Document;
using Common.Extensions;
using Ddei.Domain.CaseData.Args;
using DdeiClient.Enums;
using DdeiClient.Factories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PolarisGateway.Services.DdeiOrchestration;

public class DdeiReclassifyDocumentOrchestrationService : IDdeiReclassifyDocumentOrchestrationService
{
    private readonly IDdeiClientFactory _ddeiClientFactory;

    public DdeiReclassifyDocumentOrchestrationService(
            IDdeiClientFactory ddeiClientFactory
        )
    {
        _ddeiClientFactory = ddeiClientFactory.ExceptionIfNull();
    }

    public async Task<DocumentReclassifiedResultDto> ReclassifyDocument(DdeiReclassifyDocumentArgDto arg)
    {
        var mdsClient = _ddeiClientFactory.Create(arg.CmsAuthValues, DdeiClients.Mds);

        var caseDocumentsTask = mdsClient.ListDocumentsAsync(arg);
        var materialTypeListTask = mdsClient.GetMaterialTypeListAsync(arg);

        await Task.WhenAll(caseDocumentsTask, materialTypeListTask);

        var caseDocuments = caseDocumentsTask.Result;
        var materialTypeList = materialTypeListTask.Result;

        // TODO SH - Not found error
        var document = caseDocuments.SingleOrDefault(x => x.DocumentId == arg.DocumentId);
        // TODO SH - Not found error
        var materialType = materialTypeList.SingleOrDefault(x => x.TypeId == arg.DocumentTypeId);

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

        // TODO SH - Call Rename and RenameDescription


        return new DocumentReclassifiedResultDto
        {
            DocumentId = reclassifyResponse.ReclassifyCommunication.Id,
            DocumentTypeId = materialType.TypeId,
            OriginalDocumentTypeId = document.CmsDocType.DocumentTypeId ?? 0,
            ReclassificationType = materialType.Classification,
            // TODO SH - Implement
            //DocumentRenamed = documentRenamed,
            //DocumentRenamedOperationName = documentRenamedResult.OperationName
        };
    }

    private static ReclassificationStatement SetReclassifyDocumentStatement(MaterialTypeDto materialType, CmsDocumentDto document, DdeiReclassifyDocumentArgDto documentReclassify)
    {
        if (materialType.Classification == "STATEMENT")
        {
            var statementDate = DateTime.Parse(documentReclassify.Statement.Date);
            var statementNo = documentReclassify.Statement.StatementNo;

            if (statementNo == 0)
            {
                //var currentStatementNo = int.Parse(document.Title); // TODO SH - Check 
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