using Common.Dto.Response.Document;
using Ddei.Domain.CaseData.Args;
using System.Threading.Tasks;

namespace PolarisGateway.Services.DdeiOrchestration;

public interface IDdeiReclassifyDocumentOrchestrationService
{
    Task<(bool IsSuccess, DocumentReclassifiedResultDto Result)> ReclassifyDocument(DdeiReclassifyDocumentArgDto arg);
}