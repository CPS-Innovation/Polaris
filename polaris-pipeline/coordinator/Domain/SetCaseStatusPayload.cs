﻿using Common.Dto.Response.Documents;
using System;

namespace coordinator.Domain;

public class SetCaseStatusPayload
{
    required public int CaseId { get; set; }

    public DateTime UpdatedAt { get; set; }

    public CaseRefreshStatus Status { get; set; }

    public string FailedReason { get; set; }
}