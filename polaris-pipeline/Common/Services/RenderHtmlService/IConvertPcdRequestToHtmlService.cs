using Common.Dto.Case.PreCharge;
using System.IO;
using System.Threading.Tasks;

namespace RenderPcd
{
    public interface IConvertPcdRequestToHtmlService
    {
        public Task<Stream> ConvertAsync(PcdRequestDto pcdRequest);
    }
}
