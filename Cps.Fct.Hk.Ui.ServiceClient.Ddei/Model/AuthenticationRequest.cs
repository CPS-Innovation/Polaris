// <copyright file="AuthenticationRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;

/// <summary>
/// The authentication request.
/// </summary>
/// <param name="CorrespondenceId">The correspondence ID.</param>
/// <param name="Username">The username.</param>
/// <param name="Password">The password.</param>
public record AuthenticationRequest(
    Guid CorrespondenceId,
    string Username,
    string Password)
        : BaseRequest(CorrespondenceId: CorrespondenceId);
