﻿using System;

namespace coordinator.Domain;

public class InitiateOcrResponse
{
    public InitiateOcrResponse()
    {
    }

    public bool BlobAlreadyExists { get; set; }

    public Guid OcrOperationId { get; set; }
}