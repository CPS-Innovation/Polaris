// <copyright file="CmsValues.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Domain;

public class CmsValues
{
    public long? DocumentId { get; set; }

    public string OriginalFileName { get; set; }

    public string DocumentTypeId { get; set; }

    public string DocumentType { get; set; }

    public string FileCreatedDate { get; set; }
}
