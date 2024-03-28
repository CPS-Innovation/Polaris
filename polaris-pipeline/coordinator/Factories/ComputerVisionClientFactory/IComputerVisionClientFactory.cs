using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;

namespace coordinator.Factories.ComputerVisionClientFactory
{
    public interface IComputerVisionClientFactory
    {
        ComputerVisionClient Create();
    }
}

