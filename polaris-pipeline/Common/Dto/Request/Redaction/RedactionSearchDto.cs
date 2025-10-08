namespace Common.Dto.Request.Redaction;

public class RedactionSearchDto 
{
    public int PageIndex { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }
    public string Word { get; set; }
    public RedactionCoordinatesDto RedactionCoordinates { get; set; }
}