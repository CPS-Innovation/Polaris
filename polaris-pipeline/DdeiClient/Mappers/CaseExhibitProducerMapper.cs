using Common.Dto.Case;
using Common.Dto.Response;

namespace Ddei.Mappers
{
    public class CaseExhibitProducerMapper : ICaseExhibitProducerMapper
    {
        public ExhibitProducerDto Map(DdeiCaseDocumentExhibitProducerResponse ddeiResponse)
        {
            return new ExhibitProducerDto
            {
                Id = ddeiResponse.Id,
                ExhibitProducer = ddeiResponse.Producer
            };
        }
    }
}