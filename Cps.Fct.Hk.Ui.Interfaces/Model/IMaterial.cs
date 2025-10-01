// <copyright file="IMaterial.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

/// <summary>
/// Represents a material with properties such as ID, title, and metadata.
/// </summary>
public interface IMaterial
{
    /// <summary>
    /// Gets the unique identifier of the material.
    /// </summary>
    int MaterialId { get; }

    /// <summary>
    /// Gets or sets the title of the material.
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// Gets the original file name of the material.
    /// </summary>
    string OriginalFileName { get; }

    /// <summary>
    /// Gets or sets the document type of the material as an integer.
    /// </summary>
    int? DocumentType { get; set; }

    /// <summary>
    /// Gets the link associated with the material.
    /// </summary>
    string Link { get; }
}
