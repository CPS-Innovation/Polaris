using Common.Dto.Case;
using Ddei.Domain;
using DdeiClient.Mappers.Contract;

namespace Ddei.Mappers;

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
