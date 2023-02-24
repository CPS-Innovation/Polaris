using System;
using Microsoft.AspNetCore.Http;

namespace PolarisGateway.Mappers
{
	public interface ITrackerUrlMapper
	{
		Uri Map(HttpRequest request, Guid correlationId);
	}
}

