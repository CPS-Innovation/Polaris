// <copyright file="Redaction.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Domain;

using coordinator.Enums;
public class Redaction
{
    public MissedRedaction MissedRedaction { get; set; }

    public RedactionType RedactionType { get; set; }

    public bool ReturnedToInvestigativeAuthority { get; set; }
}
