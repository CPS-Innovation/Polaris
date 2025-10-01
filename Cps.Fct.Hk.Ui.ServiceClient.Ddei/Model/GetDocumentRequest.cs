// <copyright file="GetDocumentRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;

using System;

/// <summary>
/// The request to get the case summary.
/// </summary>
/// <param name="CaseId">The ID of the case.</param>
/// <param name="FilePath">The path of the material ID.</param>
/// <param name="CorrespondenceId">The correspondence ID.</param>
public record GetDocumentRequest(string CaseId, string FilePath, Guid CorrespondenceId)
        : BaseRequest(CorrespondenceId: CorrespondenceId)
{
}
