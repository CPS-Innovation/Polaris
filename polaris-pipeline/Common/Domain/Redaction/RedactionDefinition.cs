using System.Collections.Generic;

namespace Common.Domain.Redaction
{
    public class RedactionDefinition
    {
        public int PageIndex { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public List<RedactionCoordinates> RedactionCoordinates { get; set; }
    }
}
