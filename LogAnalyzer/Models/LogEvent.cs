using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace LogAnalyzer.Models
{
    public class LogEvent
    {
        [JsonPropertyName("event")]
        public string EventType { get; set; }

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("user_name")]
        public string UserName { get; set; }

        [JsonPropertyName("ip")]
        public string Ip { get; set; }

        [JsonPropertyName("duration_ms")]
        public int DurationMs { get; set; }

        [JsonPropertyName("success")]
        public bool? Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("amount")]
        public int? Amount { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }
    }
}
