using Common.Dto.Response;
using DdeiClient.Domain.Response;

namespace Ddei.Mappers;

public interface ICmsMaterialTypeMapper
{
    MaterialTypeDto Map(MdsMaterialTypeListResponse mdsResponse);
}