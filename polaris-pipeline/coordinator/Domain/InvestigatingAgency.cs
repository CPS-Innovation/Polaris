// <copyright file="InvestigatingAgency.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Domain;

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

public class InvestigatingAgency
{
    [JsonProperty("id")]
    [Required]
    public string Id { get; set; }

    [JsonProperty("name")]
    [Required]
    public string Name { get; set; }
}
