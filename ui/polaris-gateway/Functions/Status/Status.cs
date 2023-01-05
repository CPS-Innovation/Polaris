using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Threading.Tasks;
using RumpoleGateway.Domain.Logging;
using RumpoleGateway.Domain.Validators;
using RumpoleGateway.Extensions;

namespace RumpoleGateway.Functions.Status
{
	public class Status : BaseRumpoleFunction
	{
		private readonly ILogger<Status> _logger;
		
		public Status(ILogger<Status> logger, IAuthorizationValidator authorizationValidator)
			: base(logger, authorizationValidator)
		{
			_logger = logger;
		}

		[FunctionName("Status")]
		public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequest req)
		{
			const string loggingName = "Status - Run";
			
			var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
			if (validationResult.InvalidResponseResult != null)
				return validationResult.InvalidResponseResult;
			
			var currentCorrelationId = validationResult.CurrentCorrelationId;
			_logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);
			
			var version = Assembly
				.GetExecutingAssembly()
				.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

			var response = new Domain.Status.Status
			{
				Message = $"Gateway - Version : {version}"
			};

			_logger.LogMethodExit(currentCorrelationId, loggingName, response.ToJson());
			return new OkObjectResult(response);
		}
	}
}

