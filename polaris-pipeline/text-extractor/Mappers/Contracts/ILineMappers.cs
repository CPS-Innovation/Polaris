using Common.Domain.SearchIndex;

namespace text_extractor.Mappers.Contracts
{
    public interface ILineMapper
    {
        Line Map(Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models.Line computerVisionLine);
    }
}