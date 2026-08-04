// <copyright file="CreateRedactionLogsRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Domain;

using coordinator.Enums;
using coordinator.Validators;
using Cps.MasterDataService.Infrastructure.ApiClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reactive;
using System.Text;

public class CreateRedactionLogsRequest
{
    [Required]
    public string Urn { get; set; }

    [Required]
    [ValidateObject]
    public Unit Unit { get; set; }

    [Required]
    [ValidateObject]
    public InvestigatingAgency InvestigatingAgency { get; set; }

    [Required]
    [ValidateObject]
    public DocumentType DocumentType { get; set; }

    [Required]
    [ValidateObject]
    public List<Redaction> Redactions { get; set; }

    public string Notes { get; set; }

    [Required]
    public ChargeStatus ChargeStatus { get; set; }

    [Required]
    public CmsValues CmsValues { get; set; }
}
