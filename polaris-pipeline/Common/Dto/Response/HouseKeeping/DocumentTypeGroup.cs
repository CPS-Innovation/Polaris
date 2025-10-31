// <copyright file="DocumentTypeGroup.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping;
/// <summary>
/// Class representing document type mapped to group.
/// </summary>
public record DocumentTypeGroup
{
    /// <summary>
    /// Gets or sets the document type Id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the document type name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document type group.
    /// </summary>
    public string Group { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets document type category.
    /// </summary>
    public string Category { get; set; } = string.Empty;
}
