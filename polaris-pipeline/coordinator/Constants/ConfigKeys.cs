namespace coordinator.Constants
{
    public static class ConfigKeys
    {
      public const string CoordinatorOrchestratorTimeoutSecs = "CoordinatorOrchestratorTimeoutSecs";
      public const string SlidingClearDownInputHours = nameof(SlidingClearDownInputHours);
      public const string SlidingClearDownProtectBlobs = nameof(SlidingClearDownProtectBlobs);
      public const string SlidingClearDownBatchSize = nameof(SlidingClearDownBatchSize);
      public const string PipelineRedactPdfBaseUrl = "PolarisPipelineRedactPdfBaseUrl";
      public const string PipelineRedactorPdfBaseUrl = "PolarisPipelineRedactorPdfBaseUrl";

      public const string PipelineRedactPdfFunctionAppKey = "PolarisPipelineRedactPdfFunctionAppKey";
      public const string PipelineTextExtractorBaseUrl = "PolarisPipelineTextExtractorBaseUrl";
      public const string PipelineTextExtractorFunctionAppKey = "PolarisPipelineTextExtractorFunctionAppKey";
    }
}
