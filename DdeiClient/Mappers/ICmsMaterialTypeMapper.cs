using Common.Dto.Response;
using DdeiClient.Domain.Response;

namespace Ddei.Mappers;

public interface ICmsMaterialTypeMapper
{
    //MaterialTypeDto Map(DdeiMaterialTypeListResponse ddeiResponse);
    // Verify if this may be needed in the future
    MaterialTypeDto Map(MdsMaterialTypeListResponse mdsResponse);
}