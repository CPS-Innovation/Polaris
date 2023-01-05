using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RumpoleGateway.Domain.Logging;

namespace RumpoleGateway.Mappers
{
	public class TrackerUrlMapper : ITrackerUrlMapper
	{
		private readonly ILogger<TrackerUrlMapper> _logger;

		public TrackerUrlMapper(ILogger<TrackerUrlMapper> logger)
		{
			_logger = logger;
		}

		public Uri Map(HttpRequest request, Guid correlationId)
        {
	        _logger.LogMethodEntry(correlationId, nameof(Map), string.Empty);
	        
            var builder = new UriBuilder();
            builder.Scheme = request.Scheme;
            builder.Host = request.Host.Host;
            if (request.Host.Port.HasValue)
            {
                builder.Port = request.Host.Port.Value;
            }
            builder.Path = $"{request.Path}/tracker";

            _logger.LogMethodExit(correlationId, nameof(Map), $"Tracker Uri generated: {builder.Uri}");
            return builder.Uri;
        }
	}
}

