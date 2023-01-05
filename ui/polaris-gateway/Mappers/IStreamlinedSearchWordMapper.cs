using System;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using RumpoleGateway.Domain.RumpolePipeline;

namespace RumpoleGateway.Mappers
{
    public interface IStreamlinedSearchWordMapper
    {
        StreamlinedWord Map(Word word, string searchTerm, Guid correlationId);
    }
}
