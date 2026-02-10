using Common.Dto.Response;
using DdeiClient.Domain.Response;

namespace Ddei.Mappers;

public interface ICmsMaterialTypeMapper
{
    MaterialTypeDto Map(DdeiMaterialTypeListResponse ddeiResponse);
    MaterialTypeDto Map(MdsMaterialTypeListResponse mdsResponse);
}