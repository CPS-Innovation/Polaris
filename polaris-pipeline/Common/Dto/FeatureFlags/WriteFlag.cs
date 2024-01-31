using System.Net.Mail;
namespace Common.Dto.FeatureFlags
{
    public enum WriteFlag
    {
        Ok,
        OnlyAvailableInCms,
        DocTypeNotAllowed,
        OriginalFileTypeNotAllowed,
        IsDispatched,
        IsNotOcrProcessed,
        AttachmentCategoryNotAllowed,
    }
}