using Common.Dto.Request;
using Common.Dto.Request.DocumentManipulation;
using Common.Dto.Request.Redaction;
using System;
using System.Linq;

namespace PolarisGateway.Mappers
{
    public class RedactPdfRequestMapper : IRedactPdfRequestMapper
    {
        public RedactPdfRequestMapper()
        {
        }

        public RedactPdfRequestDto Map(DocumentRedactionSaveRequestDto saveRequest)
        {
            ArgumentNullException.ThrowIfNull(saveRequest);

            var result = new RedactPdfRequestDto
            {
                // FileName - not known yet, picked up later in the durable world
                // VersionId - not passed in previous code, possibly get set as 0->1 in Blob metadata, but as not used this isn't a problem
                RedactionDefinitions = [],
                DocumentModifications = []
            };

            foreach (var item in saveRequest.Redactions)
            {
                var redactionDefinition = new RedactionDefinitionDto
                {
                    PageIndex = item.PageIndex,
                    Height = item.Height,
                    Width = item.Width,
                    RedactionCoordinates = []
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

            if (saveRequest.DocumentModifications != null)
            {
                foreach (var item in saveRequest.DocumentModifications)
                {
                    var documentModification = new DocumentModificationDto
                    {
                        PageIndex = item.PageIndex,
                        Operation = item.Operation,
                        Arg = item.Arg
                    };

                    result.DocumentModifications.Add(documentModification);
                }
            }

            return result;
        }
    }
}
