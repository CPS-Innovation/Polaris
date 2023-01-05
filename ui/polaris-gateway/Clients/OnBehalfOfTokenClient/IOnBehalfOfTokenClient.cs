using System;
using System.Threading.Tasks;

namespace RumpoleGateway.Clients.OnBehalfOfTokenClient
{
    public interface IOnBehalfOfTokenClient
    {
        
        Task<string> GetAccessTokenAsync(string accessToken, string scope, Guid correlationId);
    }
}
