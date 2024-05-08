using Common.Dto.Request;
using Common.Dto.Request.Redaction;

namespace PolarisGateway.Mappers
{
    public class RedactPdfRequestMapper : IRedactPdfRequestMapper
    {
        public RedactPdfRequestMapper() { }

        public RedactPdfRequestDto Map(DocumentRedactionSaveRequestDto saveRequest)
        {
            if (saveRequest == null) throw new ArgumentNullException(nameof(saveRequest));

            var result = new RedactPdfRequestDto
            {
                // FileName - not known yet, picked up later in the durable world
                // VersionId - not passed in previous code, possibly get set as 0->1 in Blob metadata, but as not used this isn't a problem
                RedactionDefinitions = new List<RedactionDefinitionDto>()
            };

            foreach (var item in saveRequest.Redactions)
            {
                var redactionDefinition = new RedactionDefinitionDto
                {
                    PageIndex = item.PageIndex,
                    Height = item.Height,
                    Width = item.Width,
                    RedactionCoordinates = new List<RedactionCoordinatesDto>()
                };
                foreach (var redactionCoordinates in item.RedactionCoordinates.Select(coordinates => new RedactionCoordinatesDto
                {
                    X1 = coordinates.X1,
                    Y1 = coordinates.Y1,
                    X2 = coordinates.X2,
                    Y2 = coordinates.Y2
                }))
                {
                    redactionDefinition.RedactionCoordinates.Add(redactionCoordinates);
                }

                result.RedactionDefinitions.Add(redactionDefinition);
            }

            return result;
        }
    }
}
