// <copyright file="CreateRedactionLogsRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Request
{
    using System.ComponentModel.DataAnnotations;
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Text;
    using Common.Domain.RedactAndLog;
    using Common.Validators;
    using Cps.MasterDataService.Infrastructure.ApiClient;
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
        public List<Common.Domain.RedactAndLog.Redaction> Redactions { get; set; }

        public string Notes { get; set; }

        [Required]
        public Common.Enums.ChargeStatus ChargeStatus { get; set; }

        [Required]
        public CmsValues CmsValues { get; set; }
    }
}
