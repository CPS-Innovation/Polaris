using Ddei.Domain.CaseData.Args.Core;

namespace DdeiClient.Domain.Args;

public  class DdeiToggleIsUnusedDocumentDto : MdsDocumentArgDto
{
    public string IsUnused { get; set; }
}
