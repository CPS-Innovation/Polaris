using System;
using System.Threading.Tasks;

namespace PolarisGateway.Clients.OnBehalfOfTokenClient
{
    public interface IOnBehalfOfTokenClient
    {
        
        Task<string> GetAccessTokenAsync(string accessToken, string scope, Guid correlationId);
    }
}
