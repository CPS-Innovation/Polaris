
using pdf_redactor.Services.DocumentRedaction.Aspose;
using AsposeRedactionImplementations = pdf_redactor.Services.DocumentRedaction.Aspose.RedactionImplementations;

namespace pdf_redactor.Factories.AsposeRedactionImplementationFactory
{
  public class AsposeRedactionImplementationFactory : IAsposeRedactionImplementationFactory
  {
    private readonly IServiceProvider _serviceProvider;

    public AsposeRedactionImplementationFactory(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

    public IRedactionImplementation Create(RedactionType providerName)
    {
      switch (providerName)
      {
        case RedactionType.AsposeImage:
          return (IRedactionImplementation)_serviceProvider.GetService(typeof(AsposeRedactionImplementations.ImageConversionImplementation));
        case RedactionType.AsposeDirect:
          return (IRedactionImplementation)_serviceProvider.GetService(typeof(AsposeRedactionImplementations.DirectImplementation));
        default:
          throw new ArgumentException($"Invalid provider name {providerName}");
      }
    }
  }
}