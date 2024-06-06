namespace coordinator.Constants
{
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
    public const string ComputerVisionClientServiceKey = "ComputerVisionClientServiceKey";
    public const string ComputerVisionClientServiceUrl = "ComputerVisionClientServiceUrl";
    public const string LanguageServiceUrl = nameof(LanguageServiceUrl);
    public const string LanguageServiceKey = nameof(LanguageServiceKey);
    public const string PiiCategories = nameof(PiiCategories);
    public const string PiiChunkCharacterLimit = nameof(PiiChunkCharacterLimit);
  }
}
