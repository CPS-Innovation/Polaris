// <copyright file="ReclassifyCaseMaterialServiceRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Request.HouseKeeping;

using System;

/// <summary>
/// Represents a service-level request to reclassify a case material, containing all necessary parameters.
/// </summary>
/// <param name="CaseId">The unique identifier of the case.</param>
/// <param name="MaterialId">The unique identifier of the material to reclassify.</param>
/// <param name="Classification">The type of classification to apply.</param>
/// <param name="DocumentTypeId">The document type id.</param>
/// <param name="Used">Flag to indicate if material is used or unused.</param>
/// <param name="Subject">The material subject.</param>
/// <param name="Statement">The statement request.</param>
/// <param name="Exhibit">Exhibit request.</param>
/// <param name="CorrespondenceId">The correspondence identifier.</param>
public record ReclassifyCaseMaterialServiceRequest(
    int CaseId,
    int MaterialId,
    string Classification,
    int DocumentTypeId,
    bool Used,
    string Subject,
    ReclassifyStatementRequest? Statement = null,
    ReclassifyExhibitRequest? Exhibit = null,
    Guid CorrespondenceId = default);
