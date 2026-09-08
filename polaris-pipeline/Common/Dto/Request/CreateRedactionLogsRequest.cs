// <copyright file="CreateRedactionLogsRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Common.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reactive;
using System.Text;

namespace Common.Dto.Request
{
    public class CreateRedactionLogsRequest
    {
        [Required]
        public string Urn { get; set; }

        [Required]
        [LoggerObject]
        public Unit Unit { get; set; }

        [Required]
        [LoggerObject]
        public InvestigatingAgency InvestigatingAgency { get; set; }

        [Required]
        [LoggerObject]
        public DocumentType DocumentType { get; set; }

        [Required]
        [LoggerObject]
        public List<Redaction> Redactions { get; set; }

        public string Notes { get; set; }

        [Required]
        public ChargeStatus ChargeStatus { get; set; }

        [Required]
        public CmsValues CmsValues { get; set; }
    }
}
