using Common.Dto.Response.Documents;
using Ddei.Domain.CaseData.Args.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolarisGateway.Services.DdeiOrchestration;

public interface IDdeiCaseDocumentsOrchestrationService
{
    Task<IEnumerable<DocumentDto>> GetCaseDocuments(DdeiCaseIdentifiersArgDto arg);
}