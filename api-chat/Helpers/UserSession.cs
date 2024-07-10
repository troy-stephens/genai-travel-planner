using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_chat.Helpers
{
    using System.Text.Json.Serialization;

    public class UserSession
    {
        [JsonPropertyName("userID")]
        public string UserID { get; set; } = string.Empty;

        [JsonPropertyName("sessionID")]
        public string SessionID { get; set; } = string.Empty;

        [JsonPropertyName("detailsProvided")]
        public bool StartingDetailsProvided { get; set; } = false;

        // Parameterless constructor with default values
        public UserSession()
        {
            UserID = string.Empty;
            SessionID = string.Empty;
            StartingDetailsProvided = false;
        }
    }

}
