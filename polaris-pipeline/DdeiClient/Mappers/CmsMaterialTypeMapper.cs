using Common.Dto.Request;
using Common.Dto.Response;

namespace Ddei.Mappers
{
    public class CmsMaterialTypeMapper : ICmsMaterialTypeMapper
    {
        public MaterialTypeDto Map(DdeiMaterialTypeListResponse ddeiResponse)
        {
            return new MaterialTypeDto
            {
                TypeId = ddeiResponse.TypeId,
                Description = ddeiResponse.Description,
                NewClassificationVariant = ddeiResponse.NewClassificationVariant
            };
        }
    }
}