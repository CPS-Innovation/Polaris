﻿using text_extractor.Constants;
using text_extractor.Factories.Contracts;
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
			return new ComputerVisionClient(new ApiKeyServiceClientCredentials(_configuration[ConfigKeys.ComputerVisionClientServiceKey]))
			{
				Endpoint = _configuration[ConfigKeys.ComputerVisionClientServiceUrl]
			};
		}
	}
}
