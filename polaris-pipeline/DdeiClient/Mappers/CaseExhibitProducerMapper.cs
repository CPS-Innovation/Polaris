using Common.Dto.Document;
using Common.Dto.Response;

namespace Ddei.Mappers
{
    public class CaseExhibitProducerMapper : ICaseExhibitProducerMapper
    {
        public DocumentExhibitProducerDto Map(DdeiCaseDocumentExhibitProducerResponse ddeiResponse)
        {
            return new DocumentExhibitProducerDto
            {
                Id = ddeiResponse.Id,
                ExhibitProducer = ddeiResponse.Producer
            };
        }
    }
}