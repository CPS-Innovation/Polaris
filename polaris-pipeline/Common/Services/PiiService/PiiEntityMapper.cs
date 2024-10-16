using Azure.AI.TextAnalytics;
using Common.Services.PiiService.Domain;
using Mapster;

namespace Common.Services.PiiService
{
    public class PiiEntityMapper : IPiiEntityMapper
    {
        public PiiEntitiesResultCollection MapCollection(RecognizePiiEntitiesResultCollection recognizePiiEntitiesResults)
        {
            return recognizePiiEntitiesResults.Adapt<PiiEntitiesResultCollection>();
        }

        public PiiResultEntity MapEntity(PiiEntity piiEntity)
        {
            return piiEntity.Adapt<PiiResultEntity>();
        }
    }
}