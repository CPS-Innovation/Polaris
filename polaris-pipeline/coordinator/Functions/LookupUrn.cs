// using System;
// using System.Threading.Tasks;
// using Common.Configuration;
// using Common.Extensions;
// using coordinator.Helpers;
// using Ddei.Factories;
// using Ddei;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Azure.WebJobs;
// using Microsoft.Azure.WebJobs.Extensions.Http;
// using Microsoft.Extensions.Logging;

// namespace coordinator.Functions
// {
//     public class LookupUrn
//     {
//         private readonly ILogger<GetWitnesses> _logger;
//         private readonly IDdeiClient _ddeiClient;
//         private readonly IDdeiArgFactory _ddeiArgFactory;

//         public LookupUrn(ILogger<GetWitnesses> logger, IDdeiClient ddeiClient, IDdeiArgFactory ddeiArgFactory)
//         {
//             _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//             _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
//             _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
//         }

//         [FunctionName(nameof(LookupUrn))]
//         [ProducesResponseType(StatusCodes.Status200OK)]
//         [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//         [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//         public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.LookupUrn)] HttpRequest req,
//             int caseId)
//         {
//             Guid currentCorrelationId = default;

//             try
//             {
//                 currentCorrelationId = req.Headers.GetCorrelationId();
//                 var cmsAuthValues = req.Headers.GetCmsAuthValues();

//                 var arg = _ddeiArgFactory.CreateCaseIdArg(cmsAuthValues, currentCorrelationId, caseId);
//                 var result = await _ddeiClient.GetUrnFromCaseIdAsync(arg);

//                 return new OkObjectResult(result);
//             }
//             catch (Exception ex)
//             {
//                 return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(GetWitnesses), currentCorrelationId, ex);
//             }
//         }
//     }
// }