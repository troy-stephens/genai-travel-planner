using System.Text.Json.Serialization;

namespace console_app_function_calling.Model
{
    public class TourData
    {
        [JsonPropertyName("tour_id")]
        public string? TourId { get; set; }

        [JsonPropertyName("place_id")]
        public string? PlaceId { get; set; }

        [JsonPropertyName("place_name")]
        public string? PlaceName { get; set; }

        [JsonPropertyName("function_name")]
        public string? FunctionName { get; set; }

        [JsonPropertyName("ask")]
        public string? Ask { get; set; }
    }
}