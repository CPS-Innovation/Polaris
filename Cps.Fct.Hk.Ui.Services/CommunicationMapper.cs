// <copyright file="CommunicationMapper.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;
using System.Collections.Generic;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Interfaces.Enums;

/// <summary>
/// A service used to map DDEI value to its house keeping communication field display text.
/// </summary>
public class CommunicationMapper : ICommunicationMapper
{
    /// <summary>
    /// Communication party mappings.
    /// </summary>
    private static readonly Dictionary<string, string> PartyMapping = new ()
    {
        { "Other", nameof(CommunicationParty.Other) },
        { "Counsel", nameof(CommunicationParty.Counsel) },
        { "Court", nameof(CommunicationParty.Court) },
        { "Defence", nameof(CommunicationParty.Defence) },
        { "Police", nameof(CommunicationParty.Police) },
        { "DigitalCaseSystem", nameof(CommunicationParty.DCS) },
        { "Wcu", nameof(CommunicationParty.WCU) },
    };

    /// <summary>
    /// Communucation type/method mappings.
    /// </summary>
    private static readonly Dictionary<string, string> MethodMapping = new ()
    {
        { "EMAIL", nameof(CommunicationMethod.Email) },
        { "PHONE", nameof(CommunicationMethod.Phone) },
        { "BUNDLE", nameof(CommunicationMethod.Bundle) },
        { "PHYSICAL_ITEM", nameof(CommunicationMethod.Item) },
        { "DOCUMENT", nameof(CommunicationMethod.Document) },
        { "MEETING", nameof(CommunicationMethod.Meeting) },
    };

    /// <inheritdoc/>
    public string MapCommunicationParty(string? name)
    {
        if (name == null)
        {
            return "Unknown";
        }

        return PartyMapping.TryGetValue(name, out string? party) ? party : "Unknown";
    }

    /// <inheritdoc/>
    public string MapCommunicationMethod(string? name)
    {
        if (name == null)
        {
            return "Unknown";
        }

        return MethodMapping.TryGetValue(name, out string? method) ? method : "Unknown";
    }

    /// <inheritdoc/>
    public string MapDirection(string? name)
    {
        if (name == null)
        {
            return "Unknown";
        }

        return name == "In" ? "Incoming" : name == "Out" ? "Outgoing" : "Unknown";
    }
}
