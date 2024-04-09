
using pdf_redactor.Services.DocumentRedaction.Aspose;
using pdf_redactor.Services.DocumentRedaction.Aspose.RedactionImplementations;

namespace pdf_redactor.Factories.RedactionImplementationFactory
{
  public class RedactionImplementationFactory : IRedactionImplementationFactory
  {
    private readonly IServiceProvider _serviceProvider;

    public RedactionImplementationFactory(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

    public IRedactionImplementation Create(RedactionType providerName)
    {
      switch (providerName)
      {
        case RedactionType.AsposeImage:
          return (IRedactionImplementation)_serviceProvider.GetService(typeof(ImageConversionImplementation));
        case RedactionType.AsposeDirect:
          return (IRedactionImplementation)_serviceProvider.GetService(typeof(DirectImplementation));
        default:
          throw new ArgumentException($"Invalid provider name {providerName}");
      }
    }
  }
}