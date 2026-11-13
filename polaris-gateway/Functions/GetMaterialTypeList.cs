// <copyright file="GetMaterialTypeList.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Dto.Response;
using Common.Dto.Response.Case;
using Common.Dto.Request;
using Common.Extensions;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class GetMaterialTypeList : BaseFunction
{
    private readonly IMasterDataServiceClient masterDataServiceClient;

    public GetMaterialTypeList(
        IMasterDataServiceClient masterDataServiceClient)
    {
        this.masterDataServiceClient = masterDataServiceClient.ExceptionIfNull();
    }

    [Function(nameof(GetMaterialTypeList))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetMaterialTypeList), tags: ["Material"], Summary = "CWA - Get Material Type List", Description = "Returns material type list")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<MaterialTypeDto>), Summary = "Material Type List", Description = "Returns the list of material types")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.MaterialTypeList)] HttpRequest req, CancellationToken cancellationToken = default)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var materialTypes = await this.masterDataServiceClient.GetMaterialTypeListAsync(new CmsAuthValues(cmsAuthValues, correlationId), cancellationToken);

        return new OkObjectResult(materialTypes);
    }
}
