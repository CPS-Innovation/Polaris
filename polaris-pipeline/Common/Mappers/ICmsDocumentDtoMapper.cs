using Common.Dto.Response.Document;
using Common.Dto.Response.Document.FeatureFlags;
using Common.Dto.Response.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Mappers
{
    public interface ICmsDocumentDtoMapper
    {
        CmsDocumentDto Map(DocumentDto document, PresentationFlagsDto presentationFlagsDto);
    }
}
