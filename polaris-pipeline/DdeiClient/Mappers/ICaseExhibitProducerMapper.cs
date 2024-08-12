using Common.Dto.Case;
using Common.Dto.Response;

namespace Ddei.Mappers
{
    public interface ICaseExhibitProducerMapper
    {
        ExhibitProducerDto Map(DdeiCaseDocumentExhibitProducerResponse ddeiResponse);
    }
}