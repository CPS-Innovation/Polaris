using Common.Dto.Response;
using DdeiClient.Domain;
using DdeiClient.Domain.Response;

namespace Ddei.Mappers;

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

    public MaterialTypeDto Map(MdsMaterialTypeListResponse mdsResponse)
    {
        return new MaterialTypeDto
        {
            TypeId = int.Parse(mdsResponse.Code),
            Description = mdsResponse.Description,
            NewClassificationVariant = GetNewClassificationVariant(mdsResponse.Classification, mdsResponse.AddAsUsedOrUnused).ToString(),
            AddAsUsedOrUnused = mdsResponse.AddAsUsedOrUnused,
            Classification = mdsResponse.Classification
        };
    }

    private static NewClassificationVariant GetNewClassificationVariant(string classification, string addAsUsedOrUnused) =>
        classification switch
        {
            "STATEMENT" => NewClassificationVariant.Statement,
            "EXHIBIT" => NewClassificationVariant.Exhibit,
            _ => addAsUsedOrUnused == "Y" ? NewClassificationVariant.Other : NewClassificationVariant.Immediate
        };
}