// <copyright file="GetCmsModernTokenRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;

using System;
using Cps.Fct.Hk.Ui.Interfaces.Model;

/// <summary>
/// The request to get the CMS Modern token.
/// </summary>
/// <param name="AuthValues">The numeric case ID.</param>
/// <param name="CorrespondenceId">The correspondence ID.</param>
public record GetCmsModernTokenRequest(CmsAuthValues CmsAuthValues, Guid CorrespondenceId)
        : BaseRequest(CorrespondenceId: CorrespondenceId)
{
}

