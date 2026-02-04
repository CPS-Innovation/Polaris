using Ddei.Domain.CaseData.Args.Core;

namespace DdeiClient.Domain.Args;

public class MdsToggleIsUnusedDocumentDto : MdsDocumentArgDto
{
    public string IsUnused { get; set; }
}
