using System;
using System.Text;
using Common.Domain.SearchIndex;
using text_extractor.Factories.Contracts;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace text_extractor.Factories;

public class SearchLineFactory : ISearchLineFactory
{
    public SearchLine Create(long caseId, string documentId, long versionId, string blobName, ReadResult readResult, Common.Domain.SearchIndex.Line line, int index)
    {
        var id = $"{caseId}:{documentId}:{readResult.Page}:{index}";
        var bytes = Encoding.UTF8.GetBytes(id);
        var base64Id = Convert.ToBase64String(bytes);

        return new SearchLine(base64Id, caseId, documentId, versionId, blobName, readResult.Page, index, line.Language, line.BoundingBox,
                line.Appearance, line.Text, line.Words, readResult.Height, readResult.Width);
    }
}