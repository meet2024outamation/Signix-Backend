namespace Signix.API.Models.Messages;
using System.Text.Json.Serialization;

public class ProcessedDocument
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("original_path")]
    public string OriginalPath { get; set; }

    [JsonPropertyName("signed_path")]
    public string SignedPath { get; set; }
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; }
}

public class AcknowledgmentMessage
{
    [JsonPropertyName("signingRoomId")]
    public int SigningRoomId { get; set; }

    [JsonPropertyName("processedDocuments")]
    public List<ProcessedDocument> ProcessedDocuments { get; set; } = new();
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; }
}
