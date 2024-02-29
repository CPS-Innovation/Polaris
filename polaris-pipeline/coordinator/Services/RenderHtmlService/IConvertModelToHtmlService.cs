using System.IO;
using System.Threading.Tasks;

namespace coordinator.Services.RenderHtmlService
{
    public interface IConvertModelToHtmlService
    {
        public Task<Stream> ConvertAsync<T>(T data);
    }
}
