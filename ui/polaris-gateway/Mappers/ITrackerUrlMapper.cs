using System;
using Microsoft.AspNetCore.Http;

namespace RumpoleGateway.Mappers
{
	public interface ITrackerUrlMapper
	{
		Uri Map(HttpRequest request, Guid correlationId);
	}
}

