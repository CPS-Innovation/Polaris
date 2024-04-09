

using pdf_redactor.Services.DocumentRedaction;
using pdf_redactor.Services.DocumentRedaction.Aspose;

namespace pdf_redactor.Factories.RedactionImplementationFactory
{
  public interface IRedactionImplementationFactory
  {
    IRedactionImplementation Create(RedactionType providerName);
  }
}