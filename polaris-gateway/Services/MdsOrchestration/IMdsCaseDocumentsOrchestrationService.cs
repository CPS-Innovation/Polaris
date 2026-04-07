using Common.Dto.Response.Documents;
using Ddei.Domain.CaseData.Args.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolarisGateway.Services.MdsOrchestration;

public interface IMdsCaseDocumentsOrchestrationService
{
    Task<IEnumerable<DocumentDto>> GetCaseDocuments(MdsCaseIdentifiersArgDto arg);
}