// <copyright file="TimeProvider.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using Cps.Fct.Hk.Ui.Interfaces;

/// <summary>
/// Provides the current UTC time.
/// </summary>
public class TimeProvider : ITimeProvider
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
