namespace Common.Dto.FeatureFlags
{
    public enum WriteFlag
    {
        Ok,
        OnlyAvailableInCms,
        DocTypeNotAllowed,
        OriginalFileTypeNotAllowed,
        IsDispatched,
        // IsNotOcrProcessed is deprecated, currently in for backwards compatibility with already processed cases.
        // where durable entities already exist. Can be removed safely any time after 2024-04-13
        IsNotOcrProcessed,
        AttachmentCategoryNotAllowed,
    }
}