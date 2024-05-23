using Azure.AI.TextAnalytics;
using coordinator.Domain;
using Mapster;

namespace coordinator.Functions.DurableEntity.Entity.Mapper
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