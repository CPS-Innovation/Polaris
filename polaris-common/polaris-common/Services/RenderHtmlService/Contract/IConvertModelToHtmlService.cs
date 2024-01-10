namespace polaris_common.Services.RenderHtmlService.Contract
{
    public interface IConvertModelToHtmlService
    {
        public Task<Stream> ConvertAsync<T>(T data);
    }
}
