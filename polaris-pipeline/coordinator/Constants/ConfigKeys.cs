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
    public const string PipelineRedactorPdfBaseUrl = "PolarisPipelineRedactorPdfBaseUrl";
    public const string PipelineTextExtractorBaseUrl = "PolarisPipelineTextExtractorBaseUrl";
    public const string PiiCategories = nameof(PiiCategories);
    public const string PiiChunkCharacterLimit = nameof(PiiChunkCharacterLimit);
    public static string MdsClientTimeoutSeconds = nameof(MdsClientTimeoutSeconds);
    public static string DdeiClientTimeoutSeconds = nameof(DdeiClientTimeoutSeconds);
    public static string PdfRedactorClientTimeoutSeconds = nameof(PdfRedactorClientTimeoutSeconds);
    public static string PdfGeneratorClientTimeoutSeconds = nameof(PdfGeneratorClientTimeoutSeconds);
    public static string TextExtractorClientTimeoutSeconds = nameof(TextExtractorClientTimeoutSeconds);
}