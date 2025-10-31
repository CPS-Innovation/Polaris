// <copyright file="IDocumentTypeMapper.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using Common.Dto.Response.HouseKeeping;

/// <summary>
/// Interface for mapping document type IDs to document type names.
/// </summary>
public interface IDocumentTypeMapper
{
    /// <summary>
    /// Maps a document type ID to its corresponding document type information.
    /// </summary>
    /// <param name="documentTypeId">The document type ID to map.</param>
    /// <returns>
    /// A <see cref="DocumentTypeInfo"/> object that contains the type, classification, and document type details.
    /// If no mapping is found, a default or unknown document type information is returned.
    /// </returns>
    DocumentTypeInfo MapDocumentType(int documentTypeId);

    /// <summary>
    /// Maps a document type ID to its corresponding document type information.
    /// </summary>
    /// <param name="materialType">The material type to map.</param>
    /// <returns>
    /// A <see cref="DocumentTypeInfo"/> object that contains the type, classification, and document type details.
    /// If no mapping is found, a default or unknown document type information is returned.
    /// </returns>
    DocumentTypeInfo MapMaterialType(string materialType);

    /// <summary>
    /// Retrives all document types along their associated groups.
    /// </summary>
    /// <returns>A list of document type groups.</returns>
    IReadOnlyList<DocumentTypeGroup> GetDocumentTypesWithClassificationGroup();
}
