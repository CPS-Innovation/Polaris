using System;
using System.IO;
using Common.Domain.Document;
using Common.Models;

namespace pdf_generator.Models;

public class ReadToPdfDto : CaseDocumentVersionDto
{
    public FileType FileType { get; set; }
    public Stream Stream { get; set; }
    public Guid CorrelationId { get; set; }
    public string CmsAuthValues { get; set; }
}