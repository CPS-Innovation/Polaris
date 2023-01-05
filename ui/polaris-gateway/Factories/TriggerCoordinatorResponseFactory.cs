using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Domain.PolarisPipeline;
using PolarisGateway.Mappers;

namespace PolarisGateway.Factories
{
	public class TriggerCoordinatorResponseFactory : ITriggerCoordinatorResponseFactory
	{
        private readonly ITrackerUrlMapper _trackerUrlMapper;
        private readonly ILogger<TriggerCoordinatorResponseFactory> _logger;

        public TriggerCoordinatorResponseFactory(ITrackerUrlMapper trackerUrlMapper, ILogger<TriggerCoordinatorResponseFactory> logger)
        {
            _trackerUrlMapper = trackerUrlMapper;
            _logger = logger;
        }

		public TriggerCoordinatorResponse Create(HttpRequest request, Guid correlationId)
        {
			_logger.LogMethodEntry(correlationId, nameof(Create), string.Empty);   
            var url = _trackerUrlMapper.Map(request, correlationId);
            _logger.LogMethodExit(correlationId, nameof(Create), string.Empty);
            return new TriggerCoordinatorResponse { TrackerUrl = url };
        }
	}
}

