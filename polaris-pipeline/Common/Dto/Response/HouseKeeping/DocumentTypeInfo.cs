// <copyright file="DocumentTypeInfo.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping;

/// <summary>
/// Represents information about a document type.
/// </summary>
public class DocumentTypeInfo
{
    /// <summary>
    /// Gets or sets the document type category.
    /// </summary>
    /// <value>
    /// A string representing the document type. For example, "MG1", "ABE", etc.
    /// </value>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category of the document.
    /// </summary>
    /// <value>
    /// A string representing the category of the document. For example, "MG Form", "Statement", etc.
    /// </value>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document type group.
    /// </summary>
    public string Group { get; set; }
}
