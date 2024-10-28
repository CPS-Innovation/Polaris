using Common.Dto.Response;

namespace Ddei.Mappers
{
    public interface ICmsMaterialTypeMapper
    {
        MaterialTypeDto Map(DdeiMaterialTypeListResponse ddeiResponse);
    }
}