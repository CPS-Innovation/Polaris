using Common.Dto.Document;
using Common.Dto.Response;

namespace Ddei.Mappers
{
    public interface ICaseExhibitProducerMapper
    {
        DocumentExhibitProducerDto Map(DdeiCaseDocumentExhibitProducerResponse ddeiResponse);
    }
}