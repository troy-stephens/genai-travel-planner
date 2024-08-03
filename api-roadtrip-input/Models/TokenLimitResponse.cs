using System.Text.Json.Serialization;

namespace api_roadtrip_input.Models;

public class TokenLimitResponse
{
    [JsonPropertyName("passed_token_limit")]
    public bool? Exceeded_Token_Limit { get; set; }

    [JsonPropertyName("status_message")]
    public string? Status_Message { get; set; }

    [JsonPropertyName("token_count")]
    public string? Token_Count { get; set; }

    [JsonPropertyName("token_limit")]
    public string? Token_Limit { get; set; }
}