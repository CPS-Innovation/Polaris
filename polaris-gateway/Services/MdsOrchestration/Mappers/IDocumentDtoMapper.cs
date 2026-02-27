using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response.Document.FeatureFlags;
using Common.Dto.Response.Documents;

namespace PolarisGateway.Services.DdeiOrchestration.Mappers;

public interface IDocumentDtoMapper
{
    DocumentDto Map(CmsDocumentDto document, PresentationFlagsDto presentationFlagsDto);
    DocumentDto Map(PcdRequestCoreDto pcdRequest, PresentationFlagsDto presentationFlagsDto);
    DocumentDto Map(DefendantsAndChargesListDto defendantAndCharges, PresentationFlagsDto presentationFlagsDto);
}