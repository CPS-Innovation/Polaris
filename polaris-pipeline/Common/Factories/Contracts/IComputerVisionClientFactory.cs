using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;

namespace Common.Factories.Contracts
{
    public interface IComputerVisionClientFactory
    {
        ComputerVisionClient Create();
    }
}

