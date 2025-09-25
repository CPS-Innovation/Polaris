using DdeiClient.Services.DocumentRetrieval;

namespace DdeiClient.Factories;

public interface IDocumentRetrievalServiceFactory
{
    IDocumentRetrievalService Create(string documentId);
}