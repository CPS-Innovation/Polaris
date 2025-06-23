using System.Text.Json.Serialization;

namespace Common.Dto.Response
{
    public class MaterialTypeDto
    {
        public int TypeId { get; set; }
        public string Description { get; set; }
        public string NewClassificationVariant { get; set; }
        [JsonIgnore]
        public string AddAsUsedOrUnused { get; set; }
        [JsonIgnore]
        public string Classification { get; set; }
    }
}