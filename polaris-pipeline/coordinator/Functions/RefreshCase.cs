using Common.Configuration;
using Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using coordinator.Durable.Providers;
using coordinator.Services.CleardownService;
using coordinator.Durable.Payloads;
using Microsoft.AspNetCore.Http;
using coordinator.Helpers;
using coordinator.Domain;
using Ddei;
using Ddei.Factories;

namespace coordinator.Functions
{
    public class RefreshCase
    {
        private readonly ILogger<RefreshCase> _logger;
        private readonly IOrchestrationProvider _orchestrationProvider;
        private readonly ICleardownService _cleardownService;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly IDdeiClient _ddeiClient;

        public RefreshCase(
            ILogger<RefreshCase> logger,
            IOrchestrationProvider orchestrationProvider,
            ICleardownService cleardownService,
            IDdeiArgFactory ddeiArgFactory,
            IDdeiClient ddeiClient)
        {
            _logger = logger;
            _orchestrationProvider = orchestrationProvider;
            _cleardownService = cleardownService;
            _ddeiArgFactory = ddeiArgFactory;
            _ddeiClient = ddeiClient;
        }

        [FunctionName(nameof(RefreshCase))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status423Locked)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run
            (
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.Case)] HttpRequest req,
                string caseUrn,
                int caseId,
                [DurableClient] IDurableOrchestrationClient orchestrationClient
            )
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();
                var cmsAuthValues = req.Headers.GetCmsAuthValues();

                // #28217 - in this case we need to pre-emptively check the CMS auth values.  The policy 
                //  as it stands when calling DDEI is that we do not check beforehand and let the call itself fail
                //  if auth is missing/expired. One rationale for this is that checking auth could be an expensive operation 
                // (it probably isn't though). Also we do try to not create traffic over and above that which the user would 
                //  have created if she were using CMS to do the same work.
                //  In this case, when triggering an orchestration we hand over the auth to the durable process. It
                //  is only when the client does the follow-up polling calls do things blow up.  On balance, the cleanest
                //  thing to do is to check the auth values here.  If they blow up then the client can send the user round
                //  the reauth loop at this point. It is much more difficult to recover if the blow-up occurs later on in polling 
                //  (especially in the in-situ reauth flow).
                //  This effect is exacerbated by the fact that after #23763 we start to regularly call this operation over and above 
                //  case load and after mutations.
                //  However this code will be refactored out as part of #28158 so ¯\_(ツ)_/¯
                //  VerifyCmsAuthAsync will throw an exception if the auth values are invalid, and the HandleUnhandledException
                //  process will deal with translating to a 401 Unauthorized response. 
                await _ddeiClient.VerifyCmsAuthAsync(
                    _ddeiArgFactory.CreateCmsCaseDataArgDto(cmsAuthValues, currentCorrelationId)
                );

                var casePayload = new CaseOrchestrationPayload(caseUrn, caseId, cmsAuthValues, currentCorrelationId);
                var isAccepted = await _orchestrationProvider.RefreshCaseAsync(orchestrationClient, currentCorrelationId, caseId.ToString(), casePayload, req);

                return new ObjectResult(new RefreshCaseResponse(caseUrn, caseId))
                {
                    StatusCode = isAccepted
                        ? StatusCodes.Status200OK
                        : StatusCodes.Status423Locked
                };
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(RefreshCase), currentCorrelationId, ex);
            }
        }
    }
}