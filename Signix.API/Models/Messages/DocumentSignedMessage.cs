using System.Text.Json.Serialization;

namespace Signix.API.Models.Messages;

public class DocumentSignedMessage
{
    [JsonPropertyName("signingRoomId")]
    public int SigningRoomId { get; set; }
    [JsonPropertyName("originalPath")]
    public string OriginalPath { get; set; } = string.Empty;
    [JsonPropertyName("signedPath")]
    public string SignedPath { get; set; } = string.Empty;
    [JsonPropertyName("signData")]
    public Dictionary<string, string> SignData { get; set; } = new();
    //[JsonPropertyName("documentIds")]
    //public List<int> DocumentIds { get; set; } = new();
    [JsonPropertyName("signers")]
    public List<SignedSignerInfo> Signers { get; set; }

    [JsonPropertyName("signedDocuments")]
    public List<SignedDocumentInfo> SignedDocuments { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = "DocumentSigned";

    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}
public class SignedSignerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    [JsonPropertyName("designation")]
    public string Designation { get; set; } = string.Empty;
    [JsonPropertyName("base64SignData")]
    public string Base64SignData { get; set; } = string.Empty;
    [JsonPropertyName("signedAt")]
    public DateTime SignedAt { get; set; }
}

public class SignedDocumentInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("fileType")]
    public string FileType { get; set; } = string.Empty;

    [JsonPropertyName("fileSize")]
    public long FileSize { get; set; }

    [JsonPropertyName("docTags")]
    public Dictionary<string, object> DocTags { get; set; } = new();

    [JsonPropertyName("clientName")]
    public string? ClientName { get; set; }

    [JsonPropertyName("documentStatus")]
    public string? DocumentStatus { get; set; }
}