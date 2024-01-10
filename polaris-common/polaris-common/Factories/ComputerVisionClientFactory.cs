using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Configuration;
using polaris_common.Constants;
using polaris_common.Factories.Contracts;

namespace polaris_common.Factories
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
			return new ComputerVisionClient(new ApiKeyServiceClientCredentials(_configuration[ConfigKeys.TextExtractorKeys.ComputerVisionClientServiceKey]))
			{
				Endpoint = _configuration[ConfigKeys.TextExtractorKeys.ComputerVisionClientServiceUrl]
			};
		}
	}
}

