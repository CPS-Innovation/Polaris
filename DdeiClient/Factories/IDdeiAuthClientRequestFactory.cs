using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;

namespace Ddei.Factories;

public interface IDdeiAuthClientRequestFactory
{
    HttpRequestMessage CreateVerifyCmsAuthRequest(MdsBaseArgDto arg); // VERIFY if MdsBaseArgDto is correct here
    HttpRequestMessage CreateReclassifyDocumentRequest(MdsReclassifyDocumentArgDto arg);
    HttpRequestMessage CreateRenameDocumentRequest(MdsRenameDocumentArgDto arg);
}