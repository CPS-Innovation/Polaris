// <copyright file="PcdReviewCoreType.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Enums;

/// <summary>
/// Enum representing the type of PCD Review Core event.
/// </summary>
public enum PcdReviewCoreType
{
    /// <summary>
    /// Early advice - PCD Analysis that appears before Initial Review.
    /// </summary>
    EarlyAdvice = 0,

    /// <summary>
    /// Initial Review event type.
    /// </summary>
    InitialReview = 1,

    /// <summary>
    /// Pre-charge decision analysis event type.
    /// </summary>
    PreChargeDecisionAnalysis = 2,
}
