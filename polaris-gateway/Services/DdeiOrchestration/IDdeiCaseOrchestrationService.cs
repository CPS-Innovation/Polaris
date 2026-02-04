using Common.Dto.Response.Case;
using Ddei.Domain.CaseData.Args.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolarisGateway.Services.DdeiOrchestration;

public interface IDdeiCaseOrchestrationService
{
    Task<CaseDto> GetCase(MdsCaseIdentifiersArgDto arg);
    Task<IEnumerable<CaseDto>> GetCases(MdsUrnArgDto arg);
}