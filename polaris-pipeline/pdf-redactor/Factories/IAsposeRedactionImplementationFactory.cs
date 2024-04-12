

using pdf_redactor.Services.DocumentRedaction;
using pdf_redactor.Services.DocumentRedaction.Aspose;

namespace pdf_redactor.Factories.AsposeRedactionImplementationFactory
{
  public interface IAsposeRedactionImplementationFactory
  {
    IRedactionImplementation Create(RedactionType providerName);
  }
}