using Common.Dto.Case;
using Ddei.Domain;

namespace DdeiClient.Mappers.Contract
{
    public interface ICmsAuthValuesMapper
    {
        CmsAuthValuesDto MapCmsAuthValues(DdeiCmsAuthValuesDto cmsAuthValues);
    }
}