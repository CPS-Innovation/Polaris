// <copyright file="BulkRedactionSearchResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Domain;

using Common.Dto.Request.Redaction;
using coordinator.Enums;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using Newtonsoft.Json;

public class BulkRedactionSearchResponse
{
    public string Urn { get; set; }

    public int CaseId { get; set; }

    public string MaterialId { get; set; }

    public long DocumentId { get; set; }

    public string SearchText { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public OrchestrationProviderStatus DocumentRefreshStatus { get; set; }

    public IEnumerable<RedactionDefinitionDto> RedactionDefinitions { get; set; }

    public string FailedReason { get; set; }

    [JsonIgnore]
    public bool IsNotFound { get; set; }
}
