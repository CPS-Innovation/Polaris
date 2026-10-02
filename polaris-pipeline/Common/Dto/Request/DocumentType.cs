using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Common.Dto.Request
{
    public class DocumentType
    {
        [JsonProperty("id")]
        [Required]
        public string Id { get; set; }

        [JsonProperty("name")]
        [Required]
        public string Name { get; set; }
    }
}
