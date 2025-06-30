using Common.Configuration;
using Common.Extensions;
using coordinator.Domain;
using coordinator.Durable.Payloads;
using coordinator.Durable.Providers;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace coordinator.Functions;

public class RefreshCase
{
    private readonly ILogger<RefreshCase> _logger;
    private readonly IOrchestrationProvider _orchestrationProvider;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IDdeiAuthClient _ddeiAuthClient; 

    public RefreshCase(
        ILogger<RefreshCase> logger,
        IOrchestrationProvider orchestrationProvider,
        IDdeiArgFactory ddeiArgFactory,
        IDdeiAuthClient ddeiAuthClient)
    {
        _logger = logger;
        _orchestrationProvider = orchestrationProvider;
        _ddeiArgFactory = ddeiArgFactory;
        _ddeiAuthClient = ddeiAuthClient;
    }

    [Function(nameof(RefreshCase))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Run
    (
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.Case)] HttpRequest req,
        string caseUrn,
        int caseId,
        [DurableClient] DurableTaskClient orchestrationClient
    )
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();
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
        await _ddeiAuthClient.VerifyCmsAuthAsync(
            _ddeiArgFactory.CreateCmsCaseDataArgDto(cmsAuthValues, currentCorrelationId)
        );

        var casePayload = new CasePayload(caseUrn, caseId, cmsAuthValues, currentCorrelationId);

        var isAccepted = await _orchestrationProvider.RefreshCaseAsync(orchestrationClient, currentCorrelationId, caseId, casePayload, req);

        return new ObjectResult(new RefreshCaseResponse(caseUrn, caseId))
        {
            StatusCode = isAccepted ? StatusCodes.Status200OK : StatusCodes.Status423Locked
        };
    }
}