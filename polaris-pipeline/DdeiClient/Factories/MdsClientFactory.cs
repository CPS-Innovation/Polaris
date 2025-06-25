using Common.Extensions;
using DdeiClient.Enums;

namespace DdeiClient.Factories;

public class MdsClientFactory: IMdsClientFactory
{
    private const long MockUserId = int.MinValue;

    public string Create(string cmsAuthValues)
    {
        return cmsAuthValues.ExtractCmsUserId() == MockUserId ? nameof(DdeiClients.MdsMock) : nameof(DdeiClients.Mds);
    }
}