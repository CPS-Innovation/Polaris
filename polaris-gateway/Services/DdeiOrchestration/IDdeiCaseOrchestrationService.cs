using Common.Dto.Response.Case;
using Ddei.Domain.CaseData.Args.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDdeiCaseOrchestrationService
{
    Task<CaseDetailsDto> GetCase(DdeiCaseIdentifiersArgDto arg);
    Task<IEnumerable<CaseDto>> GetCases(DdeiUrnArgDto arg);
}