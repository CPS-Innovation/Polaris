using Common.Dto.Response.Case;
using Ddei.Domain.Response.Document;

namespace Ddei.Mappers
{
    public class CaseExhibitProducerMapper : ICaseExhibitProducerMapper
    {
        public ExhibitProducerDto Map(DdeiDocumentExhibitProducerResponse ddeiResponse)
        {
            return new ExhibitProducerDto
            {
                Id = ddeiResponse.Id,
                ExhibitProducer = ddeiResponse.Producer
            };
        }
    }
}