using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Configuration;

namespace Common.Factories.ComputerVisionClientFactory
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
			return new ComputerVisionClient(new ApiKeyServiceClientCredentials(_configuration[Constants.ComputerVisionClientServiceKey]))
			{
				Endpoint = _configuration[Constants.ComputerVisionClientServiceUrl]
			};
		}
	}
}

