using Common.Extensions;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace DdeiClient.Factories;

public class DdeiClientFactory(IServiceProvider serviceProvider) : IDdeiClientFactory
{
    private const long MockUserId = int.MinValue;

    public IDdeiClient Create(string cmsAuthValues, DdeiClients client = DdeiClients.Ddei)
    {
        if (client == DdeiClients.Mds && cmsAuthValues.ExtractCmsUserId() == MockUserId)
            client = DdeiClients.MdsMock;

        return serviceProvider.GetKeyedService<IDdeiClient>(client);
    }
}