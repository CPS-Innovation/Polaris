// <copyright file="ICommunicationMapper.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

/// <summary>
/// Interface used to map DDEI value to its house keeping communication field display text.
/// </summary>
public interface ICommunicationMapper
{
    /// <summary>
    /// Maps method to its display text.
    /// </summary>
    /// <param name="name">The method name to map.</param>
    /// <returns>The mapped method display text.</returns>
    string MapCommunicationMethod(string? name);

    /// <summary>
    /// Maps party from DDEI value to house keeping its display text.
    /// </summary>
    /// <param name="name">The party name to map.</param>
    /// <returns>The party display text.</returns>
    string MapCommunicationParty(string? name);

    /// <summary>
    /// Maps direction to its house keeping display text.
    /// </summary>
    /// <param name="name">The direction to map.</param>
    /// <returns>The direction display text.</returns>
    string MapDirection(string? name);
}
