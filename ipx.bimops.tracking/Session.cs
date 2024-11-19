using System;
using System.Text.Json.Serialization;

namespace ipx.bimops.tracking;

public class Session
{
    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }

    [JsonPropertyName("last_write")]
    public int? LastWrite { get; set; }

    [JsonPropertyName("last_read")]
    public int? LastRead { get; set; }

    [JsonPropertyName("session_active")]
    public bool? SessionActive { get; set; }
}