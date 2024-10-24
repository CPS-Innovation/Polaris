using System.Text.RegularExpressions;
using Common.Domain.Document;
using Common.Dto.Request;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;

namespace Ddei.Factories
{
    public class DdeiArgFactory : IDdeiArgFactory
    {
        public DdeiArgFactory()
        {
        }

        public DdeiBaseArgDto CreateCmsCaseDataArgDto(string cmsAuthValues, Guid correlationId)
        {
            return new DdeiBaseArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId
            };
        }

        public DdeiCaseIdOnlyArgDto CreateCaseIdArg(string cmsAuthValues, Guid correlationId, int caseId)
        {
            return new DdeiCaseIdOnlyArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                CaseId = caseId
            };
        }

        public DdeiUrnArgDto CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn)
        {
            return new DdeiUrnArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
            };
        }

        public DdeiCaseIdentifiersArgDto CreateCaseIdentifiersArg(string cmsAuthValues, Guid correlationId, string urn, int caseId)
        {
            return new DdeiCaseIdentifiersArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId
            };
        }

        public DdeiPcdArgDto CreatePcdArg(string cmsAuthValues, Guid correlationId, string urn, int caseId, int pcdId)
        {
            return new DdeiPcdArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                PcdId = pcdId
            };
        }

        public DdeiPcdArgDto CreatePcdArg(string cmsAuthValues, Guid correlationId, string urn, int caseId, string pcdId)
            => CreatePcdArg(cmsAuthValues, correlationId, urn, caseId, ConvertToDdeiPcdId(pcdId));

        public DdeiCaseIdentifiersArgDto CreateCaseArgFromUrnArg(DdeiUrnArgDto arg, int caseId)
        {
            return new DdeiCaseIdentifiersArgDto
            {
                CmsAuthValues = arg.CmsAuthValues,
                CorrelationId = arg.CorrelationId,
                Urn = arg.Urn,
                CaseId = caseId
            };
        }

        public DdeiDocumentArgDto CreateDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId)
        {
            return new DdeiDocumentArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId
            };
        }

        public DdeiDocumentArgDto CreateDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId)
            => CreateDocumentArgDto(cmsAuthValues, correlationId, urn, caseId, ConvertToDdeiDocumentId(documentId));

        public DdeiDocumentIdAndVersionIdArgDto CreateDocumentVersionArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, long versionId)
        {
            return new DdeiDocumentIdAndVersionIdArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId,
                VersionId = versionId
            };
        }

        public DdeiDocumentIdAndVersionIdArgDto CreateDocumentVersionArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId)
            => CreateDocumentVersionArgDto(cmsAuthValues, correlationId, urn, caseId, ConvertToDdeiDocumentId(documentId), versionId);

        public DdeiAddDocumentNoteArgDto CreateAddDocumentNoteArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, string text)
        {
            return new DdeiAddDocumentNoteArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId,
                Text = text
            };
        }

        public DdeiAddDocumentNoteArgDto CreateAddDocumentNoteArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, string text)
            => CreateAddDocumentNoteArgDto(cmsAuthValues, correlationId, urn, caseId, ConvertToDdeiDocumentId(documentId), text);

        public DdeiRenameDocumentArgDto CreateRenameDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, string documentName)
        {
            return new DdeiRenameDocumentArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId,
                DocumentName = documentName
            };
        }

        public DdeiRenameDocumentArgDto CreateRenameDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, string documentName)
            => CreateRenameDocumentArgDto(cmsAuthValues, correlationId, urn, caseId, ConvertToDdeiDocumentId(documentId), documentName);

        public DdeiReclassifyDocumentArgDto CreateReclassifyDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, long documentId, ReclassifyDocumentDto dto)
        {
            return new DdeiReclassifyDocumentArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                DocumentId = documentId,
                DocumentTypeId = dto.DocumentTypeId,
                Exhibit = dto.Exhibit,
                Statement = dto.Statement,
                Other = dto.Other,
                Immediate = dto.Immediate
            };
        }

        public DdeiReclassifyDocumentArgDto CreateReclassifyDocumentArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, ReclassifyDocumentDto dto)
            => CreateReclassifyDocumentArgDto(cmsAuthValues, correlationId, urn, caseId, ConvertToDdeiDocumentId(documentId), dto);

        public DdeiWitnessStatementsArgDto CreateWitnessStatementsArgDto(string cmsAuthValues, Guid correlationId, string urn, int caseId, int witnessId)
        {
            return new DdeiWitnessStatementsArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = urn,
                CaseId = caseId,
                WitnessId = witnessId
            };
        }

        private static long ConvertToDdeiDocumentId(string documentId)
        {
            if (string.IsNullOrWhiteSpace(documentId))
            {
                throw new ArgumentNullException(nameof(documentId));
            }

            var match = Regex.Match(
                documentId,
                $@"{DocumentNature.DocumentPrefix}-(\d+)",
                RegexOptions.None,
                TimeSpan.FromSeconds(1));

            return match.Success
                ? long.Parse(match.Groups[1].Value)
                : throw new ArgumentException($"Invalid document id: {documentId}. Expected format e.g.: '{DocumentNature.DocumentPrefix}-123456'");
        }

        private static int ConvertToDdeiPcdId(string documentId)
        {
            if (string.IsNullOrWhiteSpace(documentId))
            {
                throw new ArgumentNullException(nameof(documentId));
            }

            var match = Regex.Match(
                documentId,
                $@"{DocumentNature.PreChargeDecisionRequestPrefix}-(\d+)",
                RegexOptions.None,
                TimeSpan.FromSeconds(1));

            return match.Success
                ? int.Parse(match.Groups[1].Value)
                : throw new ArgumentException($"Invalid document id: {documentId}. Expected format e.g.: '{DocumentNature.PreChargeDecisionRequestPrefix}-123456'");
        }
    }
}