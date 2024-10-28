using Common.Dto.Response.Documents;
using Ddei.Domain.CaseData.Args.Core;

namespace PolarisGateway.Services.DdeiOrchestration;

public interface IDdeiOrchestrationService
{
    Task<IEnumerable<DocumentDto>> GetCaseDocuments(DdeiCaseIdentifiersArgDto arg);
}