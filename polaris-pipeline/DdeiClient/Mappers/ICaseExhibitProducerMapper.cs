using Common.Dto.Response.Case;
using Ddei.Domain.Response.Document;


namespace Ddei.Mappers
{
    public interface ICaseExhibitProducerMapper
    {
        ExhibitProducerDto Map(DdeiDocumentExhibitProducerResponse ddeiResponse);
    }
}