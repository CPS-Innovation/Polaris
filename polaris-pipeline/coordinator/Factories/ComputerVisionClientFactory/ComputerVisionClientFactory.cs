using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Configuration;
using coordinator.Constants;

namespace coordinator.Factories.ComputerVisionClientFactory
{
	public class ComputerVisionClientFactory : IComputerVisionClientFactory
	{
		private readonly IConfiguration _configuration;

		public ComputerVisionClientFactory(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public ComputerVisionClient Create()
		{
			return new ComputerVisionClient(new ApiKeyServiceClientCredentials(_configuration[ConfigKeys.ComputerVisionClientServiceKey]))
			{
				Endpoint = _configuration[ConfigKeys.ComputerVisionClientServiceUrl]
			};
		}
	}
}

