// <copyright file="BaseRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;

using System;

/// <summary>
/// The correspondence request.
/// </summary>
/// <param name="CorrespondenceId">The correspondence ID.</param>
public record BaseRequest(Guid CorrespondenceId);
