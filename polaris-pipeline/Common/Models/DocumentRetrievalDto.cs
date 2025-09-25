using Common.Domain.Document;
using System.IO;

namespace PolarisGateway.Models;

public class DocumentRetrievalDto
{
    public Stream Stream { get; set; }
    public FileType FileType  {get;set;}
    public bool IsKnownFileType {get;set;}
}