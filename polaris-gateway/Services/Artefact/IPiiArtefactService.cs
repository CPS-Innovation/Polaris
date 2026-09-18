// <copyright file="IPiiArtefactService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Services.Artefact;

using Common.Domain.Pii;
using PolarisGateway.Services.Artefact.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IPiiArtefactService
{
    public Task<ArtefactResult<IEnumerable<PiiLine>>> GetPiiAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId, long documentId, bool isOcrProcessed, Guid? operationId = null, bool forceRefresh = false, bool isLegacy = true);
}
