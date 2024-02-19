using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;

namespace text_extractor.Factories.Contracts
{
    public interface IComputerVisionClientFactory
    {
        ComputerVisionClient Create();
    }
}

