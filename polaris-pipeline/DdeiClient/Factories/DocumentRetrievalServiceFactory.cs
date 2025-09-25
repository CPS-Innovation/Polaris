using Common.Domain.Document;
using DdeiClient.Services.DocumentRetrieval;
using Microsoft.Extensions.DependencyInjection;

namespace DdeiClient.Factories;

public class DocumentRetrievalServiceFactory : IDocumentRetrievalServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public DocumentRetrievalServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IDocumentRetrievalService Create(string documentId)
    {
        var documentNature = DocumentNature.GetDocumentNatureType(documentId);
        return _serviceProvider.GetKeyedService<IDocumentRetrievalService>(documentNature);
    }
}