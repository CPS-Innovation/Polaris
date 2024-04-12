

using pdf_redactor.Services.DocumentRedaction;
using pdf_redactor.Services.DocumentRedaction.SyncFusion;

namespace pdf_redactor.Factories.SyncFusionRedactionImplementationFactory
{
  public interface ISyncFusionRedactionImplementationFactory
  {
    IRedactionImplementation Create(RedactionType providerName);
  }
}