using Azure.AI.TextAnalytics;
using coordinator.Domain;

namespace coordinator.Functions.DurableEntity.Entity.Mapper
{
    public interface IPiiEntityMapper
    {
        public PiiResultEntity MapEntity(PiiEntity piiEntity);
        public PiiEntitiesResultCollection MapCollection(RecognizePiiEntitiesResultCollection recognizePiiEntitiesResults);
    }
}