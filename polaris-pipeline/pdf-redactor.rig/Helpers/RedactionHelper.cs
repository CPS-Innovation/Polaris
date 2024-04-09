using Aspose.Pdf;
using Aspose.Pdf.Facades;
using Common.Dto.Request;
using Common.Dto.Request.Redaction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class RedactionHelper
{
  public static RedactPdfRequestWithDocumentDto LoadRedactionDataForPdf(Document document, Stream pdfStream, string fileName)
  {
    var redactionDefinitions = new List<RedactionDefinitionDto>();
    for (int pageIndex = 1; pageIndex <= document.Pages.Count; pageIndex++)
    {
      var page = document.Pages[pageIndex];
      var pdfInfo = new PdfFileInfo(document);

      var redactionDefinition = new RedactionDefinitionDto
      {
        PageIndex = pageIndex,
        Width = pdfInfo.GetPageWidth(pageIndex),
        Height = pdfInfo.GetPageHeight(pageIndex),
        RedactionCoordinates = new List<RedactionCoordinatesDto>
                {
                    new RedactionCoordinatesDto
                    {
                        X1 = 100,
                        Y1 = 150,
                        X2 = 150,
                        Y2 = 200
                    }
                }
      };

      redactionDefinitions.Add(redactionDefinition);
    }

    using var documentReader = new BinaryReader(pdfStream);
    var documentBytes = documentReader.ReadBytes((int)pdfStream.Length);
    var base64Document = Convert.ToBase64String(documentBytes);

    return new RedactPdfRequestWithDocumentDto
    {
      FileName = fileName,
      Document = base64Document,
      VersionId = 1,
      RedactionDefinitions = redactionDefinitions
    };
  }
}
