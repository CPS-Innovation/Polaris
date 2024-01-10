using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;

namespace polaris_common.Factories.Contracts
{
    public interface IComputerVisionClientFactory
    {
        ComputerVisionClient Create();
    }
}

