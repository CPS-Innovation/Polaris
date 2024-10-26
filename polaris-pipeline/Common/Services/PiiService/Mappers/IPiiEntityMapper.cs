using Azure.AI.TextAnalytics;
using Common.Services.PiiService.Domain;

namespace Common.Services.PiiService.Mappers
{
    public interface IPiiEntityMapper
    {
        public PiiResultEntity MapEntity(PiiEntity piiEntity);
        public PiiEntitiesResultCollection MapCollection(RecognizePiiEntitiesResultCollection recognizePiiEntitiesResults);
    }
}