﻿namespace api_chat.Models
{
    public class ChatProviderResponseDto
    {
        public string? StatusMessage { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string? Result { get; set; } = string.Empty;
    }
}
