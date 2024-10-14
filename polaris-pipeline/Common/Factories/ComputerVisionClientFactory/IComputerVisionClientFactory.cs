using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;

namespace Common.Factories.ComputerVisionClientFactory
{
    public interface IComputerVisionClientFactory
    {
        ComputerVisionClient Create();
    }
}

