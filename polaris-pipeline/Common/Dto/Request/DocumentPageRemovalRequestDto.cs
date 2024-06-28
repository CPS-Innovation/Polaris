using Newtonsoft.Json;

namespace Common.Dto.Request
{
    public class DocumentPageRemovalRequestDto
    {
        [JsonProperty("pageIndexesToRemove")]
        public int[] PagesIndexesToRemove { get; set; }
    }
}