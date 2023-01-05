using System;
using System.Threading.Tasks;

namespace Common.Adapters
{
    public interface IIdentityClientAdapter
    {
        Task<string> GetAccessTokenOnBehalfOfAsync(string currentAccessToken, string scopes, Guid correlationId);

        Task<string> GetClientAccessTokenAsync(string scopes, Guid correlationId);
    }
}
