using System;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using PolarisGateway.Domain.PolarisPipeline;

namespace PolarisGateway.Mappers
{
    public interface IStreamlinedSearchWordMapper
    {
        StreamlinedWord Map(Word word, string searchTerm, Guid correlationId);
    }
}
