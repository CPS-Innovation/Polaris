
using pdf_redactor.Services.DocumentRedaction.SyncFusion;
using SyncFusionRedactionImplementations = pdf_redactor.Services.DocumentRedaction.SyncFusion.RedactionImplementations;

namespace pdf_redactor.Factories.SyncFusionRedactionImplementationFactory
{
  public class SyncFusionRedactionImplementationFactory : ISyncFusionRedactionImplementationFactory
  {
    private readonly IServiceProvider _serviceProvider;

    public SyncFusionRedactionImplementationFactory(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

    public IRedactionImplementation Create(RedactionType providerName)
    {
      switch (providerName)
      {
        case RedactionType.SyncFusion:
          return (IRedactionImplementation)_serviceProvider.GetService(typeof(SyncFusionRedactionImplementations.DirectImplementation));
        case RedactionType.SyncFusionImage:
          return (IRedactionImplementation)_serviceProvider.GetService(typeof(SyncFusionRedactionImplementations.ImageConversionImplementation));
        default:
          throw new ArgumentException($"Invalid provider name {providerName}");
      }
    }
  }
}