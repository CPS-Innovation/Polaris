// <copyright file="ConfigKeys.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Constants;

public static class ConfigKeys
{
    public const string CoordinatorOrchestratorTimeoutSecs = "CoordinatorOrchestratorTimeoutSecs";
    public const string CoordinatorSwitchoverCaseId = nameof(CoordinatorSwitchoverCaseId);
    public const string CoordinatorSwitchoverModulo = nameof(CoordinatorSwitchoverModulo);
    public const string SlidingClearDownInputHours = nameof(SlidingClearDownInputHours);
    public const string SlidingClearDownProtectBlobs = nameof(SlidingClearDownProtectBlobs);
    public const string SlidingClearDownBatchSize = nameof(SlidingClearDownBatchSize);
    public const string PipelineRedactPdfBaseUrl = "PolarisPipelineRedactPdfBaseUrl";
    public const string PipelineTextExtractorBaseUrl = "PolarisPipelineTextExtractorBaseUrl";
    public const string PiiCategories = nameof(PiiCategories);
    public const string PiiChunkCharacterLimit = nameof(PiiChunkCharacterLimit);
    public const string MdsClientTimeoutSeconds = nameof(MdsClientTimeoutSeconds);
    public const string DdeiClientTimeoutSeconds = nameof(DdeiClientTimeoutSeconds);
    public const string RedactionLoggerBaseUrl = "RedactionLogger:BaseUrl";
    public const string RedactionLoggerTimeoutSeconds = "RedactionLogger:TimeoutSeconds";
    public const string RedactionLoggerMaxRetries = "RedactionLogger:MaxRetries";
    public const string RedactionLoggerAccessKey = "RedactionLogger:AccessKey";
    public const string RedactorBaseUrl = "Redactor:BaseUrl";
    public const string RedactorTimeoutSeconds = "Redactor:TimeoutSeconds";
    public const string RedactorMaxRetries = "Redactor:MaxRetries";
    public const string RedactorAccessKey = "Redactor:AccessKey";
    public const string PdfGeneratorClientTimeoutSeconds = nameof(PdfGeneratorClientTimeoutSeconds);
    public const string TextExtractorClientTimeoutSeconds = nameof(TextExtractorClientTimeoutSeconds);
}
