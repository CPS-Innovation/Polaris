using System;
using System.Text;
using Common.Domain.SearchIndex;
using Common.Factories.Contracts;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Common.Factories;

public class SearchLineFactory : ISearchLineFactory
{
    public SearchLine Create(Guid polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobName, ReadResult readResult, Line line, int index)
    {
        var indexerId = $"{polarisDocumentId}-{readResult.Page}-{index}";
        var bytes = Encoding.UTF8.GetBytes(indexerId);
        var base64IndexerId = Convert.ToBase64String(bytes);

        return new SearchLine
            (
                base64IndexerId,
                polarisDocumentId, 
                blobName, 
                readResult.Page, 
                index, 
                line.Language, 
                line.BoundingBox, 
	            line.Appearance, 
                line.Text, 
                line.Words, 
                readResult.Height, 
                readResult.Width
            );
    }
}