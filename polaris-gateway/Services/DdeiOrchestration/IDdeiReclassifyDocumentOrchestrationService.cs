using Common.Dto.Response.Document;
using Ddei.Domain.CaseData.Args;
using System.Threading.Tasks;

namespace PolarisGateway.Services.DdeiOrchestration;

public interface IDdeiReclassifyDocumentOrchestrationService
{
    Task<DocumentReclassifiedResultDto> ReclassifyDocument(DdeiReclassifyDocumentArgDto arg);
}