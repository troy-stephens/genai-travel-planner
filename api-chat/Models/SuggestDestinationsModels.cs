using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace api_chat.Models
{
    public class CheckForDetails
    {
        [JsonPropertyName("detailsProvided")]
        public bool DetailsProvided { get; set; } = false;

        [JsonPropertyName("startingPoint")]
        public string? StartingPoint { get; set; } = string.Empty;

        [JsonPropertyName("destination")]
        public string? Destination { get; set; } = string.Empty;

        [JsonPropertyName("duration")]
        public string? Duration { get; set; } = string.Empty;

        [JsonPropertyName("activities")]
        public string Activities { get; set; } = string.Empty;
    }

}
