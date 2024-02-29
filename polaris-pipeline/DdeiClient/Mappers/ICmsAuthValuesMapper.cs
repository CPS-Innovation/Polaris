using Common.Dto.Case;
using Ddei.Domain;

namespace DdeiClient.Mappers
{
    public interface ICmsAuthValuesMapper
    {
        CmsAuthValuesDto MapCmsAuthValues(DdeiCmsAuthValuesDto cmsAuthValues);
    }
}