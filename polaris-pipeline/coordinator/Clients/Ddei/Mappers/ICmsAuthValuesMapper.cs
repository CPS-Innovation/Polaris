using Common.Dto.Case;
using coordinator.Clients.Ddei.Domain;

namespace coordinator.Clients.Ddei.Mappers
{
    public interface ICmsAuthValuesMapper
    {
        CmsAuthValuesDto MapCmsAuthValues(DdeiCmsAuthValues cmsAuthValues);
    }
}