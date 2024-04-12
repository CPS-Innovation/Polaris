
using pdf_redactor.Services.DocumentRedaction;
using pdf_redactor.Services.DocumentRedaction.Aspose;
using pdf_redactor.Services.DocumentRedaction.SyncFusion;

namespace pdf_redactor.Factories.RedactionProviderFactory
{
  public class RedactionProviderFactory : IRedactionProviderFactory
  {
    private readonly IServiceProvider _serviceProvider;

    public RedactionProviderFactory(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

    public IRedactionProvider Create(RedactionType providerName)
    {
      switch (providerName)
      {
        case RedactionType.AsposeImage:
        case RedactionType.AsposeDirect:
          return (IRedactionProvider)_serviceProvider.GetService(typeof(AsposeRedactionProvider));
        case RedactionType.SyncFusion:
        case RedactionType.SyncFusionImage:
          return (IRedactionProvider)_serviceProvider.GetService(typeof(SyncFusionRedactionProvider));
        default:
          throw new ArgumentException($"Invalid provider name {providerName}");
      }
    }
  }
}