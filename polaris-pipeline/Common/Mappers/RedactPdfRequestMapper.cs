using System;
using System.Collections.Generic;
using System.Linq;
using Common.Dto.Request;
using Common.Dto.Request.Redaction;
using Common.Mappers.Contracts;
using Common.ValueObjects;

namespace Common.Mappers
{
    public class RedactPdfRequestMapper : IRedactPdfRequestMapper
    {
        public RedactPdfRequestDto Map(DocumentRedactionSaveRequestDto saveRequest, long caseId, PolarisDocumentId polarisDocumentId, Guid correlationId)
        {
            if (saveRequest == null) throw new ArgumentNullException(nameof(saveRequest));

            var result = new RedactPdfRequestDto
            {
                CaseId = caseId,
                PolarisDocumentId = polarisDocumentId,
                // FileName - not known yet, picked up later in the durable world
                // VersionId - not passed in previous code, possibly get set as 0->1 in Bob metadata, but as not used this isn't a problem
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
