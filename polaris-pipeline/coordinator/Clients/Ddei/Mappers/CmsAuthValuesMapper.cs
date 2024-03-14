using Common.Dto.Case;
using coordinator.Clients.Ddei.Domain;
using coordinator.Clients.Ddei.Mappers;

namespace coordinator.Clients.Ddei.Mappers;

public class CmsAuthValuesMapper : ICmsAuthValuesMapper
{
    public CmsAuthValuesDto MapCmsAuthValues(DdeiCmsAuthValuesDto cmsAuthValues)
    {
        return new CmsAuthValuesDto
        {
            Cookies = cmsAuthValues.Cookies,
            UserIpAddress = cmsAuthValues.UserIpAddress,
            PreferredLoadBalancerTarget = cmsAuthValues.PreferredLoadBalancerTarget,
            Token = cmsAuthValues.Token,
        };
    }
}
