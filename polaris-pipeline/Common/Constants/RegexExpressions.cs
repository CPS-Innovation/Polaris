// <copyright file="RegexExpressions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Constants;

/// <summary>
/// Class to store all regex expressions.
/// </summary>
public static class RegexExpressions
{
    /// <summary>
    /// Regex for allowing certain special characters in material name.
    /// </summary>
    public const string RenameMaterialSubjectRegex = @"^[a-zA-Z0-9,\-\)\(_\/:=\'.""\~\s]+$";
}
