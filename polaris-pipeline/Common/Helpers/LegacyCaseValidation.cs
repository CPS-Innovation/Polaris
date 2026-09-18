namespace Common.Helpers;

using System;

public static class LegacyCaseValidation
{
    public static void EnsureCaseUrnProvided(string caseUrn, bool isLegacy)
    {
        if (isLegacy && string.IsNullOrWhiteSpace(caseUrn))
        {
            throw new ArgumentNullException(nameof(caseUrn));
        }
    }
}