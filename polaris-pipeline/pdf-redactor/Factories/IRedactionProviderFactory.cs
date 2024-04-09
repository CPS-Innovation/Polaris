

using pdf_redactor.Services.DocumentRedaction;

namespace pdf_redactor.Factories.RedactionProviderFactory
{
  public interface IRedactionProviderFactory
  {
    IRedactionProvider Create(RedactionType providerName);
  }
}