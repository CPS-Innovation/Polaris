using Common.Dto.Request.Redaction;

public class RedactionData
{
  public required string DocumentId { get; set; }
  public required List<RedactionDefinitionDto> Redactions { get; set; }
}
