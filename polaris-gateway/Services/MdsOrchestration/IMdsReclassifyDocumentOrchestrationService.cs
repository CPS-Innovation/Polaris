using Common.Dto.Response.Document;
using Ddei.Domain.CaseData.Args;
using System.Threading.Tasks;

namespace PolarisGateway.Services.MdsOrchestration;

public interface IMdsReclassifyDocumentOrchestrationService
{
    Task<DocumentReclassifiedResult> ReclassifyDocument(MdsReclassifyDocumentArgDto arg);
}