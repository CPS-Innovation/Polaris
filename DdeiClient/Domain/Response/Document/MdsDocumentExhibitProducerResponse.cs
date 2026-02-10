using Ddei.Domain.Response.Document;

namespace DdeiClient.Domain.Response.Document;

public class MdsDocumentExhibitProducerResponse
{
    public IEnumerable<DdeiDocumentExhibitProducerResponse> ExhibitProducers { get; set; }
}