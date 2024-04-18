
using Common.Dto.Request;
using Common.Dto.Request.Redaction;
using System.Text.Json;

public class RedactionHelper
{
  public static RedactPdfRequestWithDocumentDto LoadRedactionDataForPdf(Stream redactionJsonStream, Stream pdfStream, string fileName)
  {

    using var streamReader = new StreamReader(redactionJsonStream);
    var jsonText = streamReader.ReadToEnd();
    var redactionData = JsonSerializer.Deserialize<RedactionData>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new Exception("Failed to deserialize redaction data");

    var redactionDefinitions = new List<RedactionDefinitionDto>();
    foreach (var redaction in redactionData.Redactions)
    {
      var redactionDefinition = new RedactionDefinitionDto
      {
        PageIndex = redaction.PageIndex,
        Width = redaction.Width,
        Height = redaction.Height,
        RedactionCoordinates = redaction.RedactionCoordinates.Select(rc => new RedactionCoordinatesDto
        {
          X1 = rc.X1,
          Y1 = rc.Y1,
          X2 = rc.X2,
          Y2 = rc.Y2
        }).ToList()
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