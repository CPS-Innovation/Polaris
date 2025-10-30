using Ddei.Domain.CaseData.Args.Core;

namespace DdeiClient.Domain.Args;

public  class DdeiToggleIsUnusedDocumentDto : DdeiDocumentArgDto
{
    public string IsUnused { get; set; }
}
