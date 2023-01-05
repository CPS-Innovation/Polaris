using Common.Constants;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Configuration;

namespace text_extractor.Factories
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

