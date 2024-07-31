using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace api_sanitize_validate.Models
{
    public class RoadTripRoot
    {
        [JsonPropertyName("vendor")]
        public string? Vendor { get; set; }

        [JsonPropertyName("road_trip")]
        public RoadTrip? RoadTrip { get; set; }
    }

    public class RoadTrip
    {
        [JsonPropertyName("start")]
        public string? Start { get; set; }

        [JsonPropertyName("end")]
        public string? End { get; set; }

        [JsonPropertyName("points_of_interest")]
        public List<PointOfInterest>? PointsOfInterest { get; set; }
    }

    public class PointOfInterest
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("opening_hours")]
        public OpeningHours? OpeningHours { get; set; }

        [JsonPropertyName("costs")]
        [JsonConverter(typeof(CostsConverter))]
        public Costs? Costs { get; set; }  // Use the custom Costs type

        [JsonPropertyName("website")]
        public string? Website { get; set; }

        [JsonPropertyName("contact")]
        public string? Contact { get; set; }
    }

    public class OpeningHours
    {
        public string? Monday { get; set; }
        public string? Tuesday { get; set; }
        public string? Wednesday { get; set; }
        public string? Thursday { get; set; }
        public string? Friday { get; set; }
        public string? Saturday { get; set; }
        public string? Sunday { get; set; }
    }

    public class Costs
    {
        public Dictionary<string, string>? CostDetails { get; set; }
        public string? FreeText { get; set; }

        public bool IsFree => !string.IsNullOrEmpty(FreeText) && FreeText.Equals("Free", StringComparison.OrdinalIgnoreCase);
    }

    public class CostsConverter : JsonConverter<Costs>
    {
        public override Costs Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                // Handle case where costs is just a string
                string value = reader.GetString() ?? "";
                return new Costs { FreeText = value };
            }

            // Handle case where costs is a dictionary
            var costDetails = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options);
            return new Costs { CostDetails = costDetails };
        }

        public override void Write(Utf8JsonWriter writer, Costs value, JsonSerializerOptions options)
        {
            if (value.IsFree)
            {
                writer.WriteStringValue(value.FreeText);
            }
            else if (value.CostDetails != null)
            {
                JsonSerializer.Serialize(writer, value.CostDetails, options);
            }
        }
    }

}

