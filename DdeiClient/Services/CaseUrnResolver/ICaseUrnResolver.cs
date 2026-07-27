// <copyright file="ICaseUrnResolver.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace DdeiClient.Services.CaseUrnResolver;

using Common.Dto.Request;
using System;
using System.Threading;
using System.Threading.Tasks;

public interface ICaseUrnResolver
{
    Task<string> ResolveCaseUrnAsync(int caseId, CmsAuthValues cmsAuthValues, CancellationToken cancellationToken = default);
}
