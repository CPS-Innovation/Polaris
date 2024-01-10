namespace polaris_common.Services.SasGeneratorService
{
    public interface ISasGeneratorService
    {
        Task<string> GenerateSasUrlAsync(string blobName, Guid correlationId);
    }
}
