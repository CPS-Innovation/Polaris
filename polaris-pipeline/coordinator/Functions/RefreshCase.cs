// <copyright file="RefreshCase.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Functions;

using Common.Configuration;
using Common.Dto.Request;
using Common.Extensions;
using coordinator.Domain;
using coordinator.Durable.Payloads;
using coordinator.Durable.Providers;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Services.CaseUrnResolver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

public class RefreshCase(
    IOrchestrationProvider orchestrationProvider,
    IMdsArgFactory mdsArgFactory,
    IDdeiAuthClient ddeiAuthClient,
    ICaseUrnResolver caseUrnResolver)
{
    [Function(nameof(RefreshCase))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Run
    (
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.Case)] HttpRequest req,
        int caseId,
        CancellationToken cancellationToken,
        [DurableClient] DurableTaskClient orchestrationClient
    )
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        var caseUrn = await caseUrnResolver.ResolveCaseUrnAsync(caseId, cmsAuthValues, cancellationToken);

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
        await ddeiAuthClient.VerifyCmsAuthAsync(
            mdsArgFactory.CreateCmsCaseDataArgDto(cmsAuthValues.CmsAuthFullValue, currentCorrelationId));

        var casePayload = new CasePayload(caseUrn, caseId, cmsAuthValues.CmsAuthFullValue, currentCorrelationId);

        var isAccepted = await orchestrationProvider.RefreshCaseAsync(orchestrationClient, currentCorrelationId, caseId, casePayload, req);

        return new ObjectResult(new RefreshCaseResponse(caseUrn, caseId))
        {
            StatusCode = isAccepted ? StatusCodes.Status200OK : StatusCodes.Status423Locked,
        };
    }
}
