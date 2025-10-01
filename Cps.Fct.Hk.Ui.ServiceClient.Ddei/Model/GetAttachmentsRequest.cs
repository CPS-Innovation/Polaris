// <copyright file="GetAttachmentsRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;

using System;

/// <summary>
/// The request to get the attachments for a communication.
/// </summary>
/// <param name="CommunicationId">The numeric communication ID.</param>
/// <param name="CorrespondenceId">The correspondence ID.</param>
public record GetAttachmentsRequest(int CommunicationId, Guid CorrespondenceId)
        : BaseRequest(CorrespondenceId: CorrespondenceId)
{
}

