using text_extractor.Mappers.Contracts;

namespace text_extractor.Mappers
{
    public class LineMapper : ILineMapper
    {
        public Common.Domain.SearchIndex.Line Map(Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models.Line computerVisionLine)
        {
            return new Common.Domain.SearchIndex.Line(
                computerVisionLine.BoundingBox,
                computerVisionLine.Text,
                computerVisionLine.Words,
                computerVisionLine.Language,
                computerVisionLine.Appearance);
        }
    }
}