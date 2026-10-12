// <copyright file="ICleardownService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Services.ClearDownService;

using Microsoft.DurableTask.Client;
using System;
using System.Threading.Tasks;

public interface IClearDownService
{
    Task DeleteCaseAsync(DurableTaskClient client, string caseUrn, int caseId, Guid correlationId, bool isLegacy = true, bool removeBlobs = true);
}
