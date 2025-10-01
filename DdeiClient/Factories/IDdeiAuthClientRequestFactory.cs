using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;

namespace Ddei.Factories;

public interface IDdeiAuthClientRequestFactory
{
    HttpRequestMessage CreateVerifyCmsAuthRequest(DdeiBaseArgDto arg);
    HttpRequestMessage CreateReclassifyDocumentRequest(DdeiReclassifyDocumentArgDto arg);
    HttpRequestMessage CreateRenameDocumentRequest(DdeiRenameDocumentArgDto arg);
}