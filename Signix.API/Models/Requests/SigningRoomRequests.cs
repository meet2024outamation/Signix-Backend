using System.Text.Json;
using System.Text.Json.Serialization;

namespace Signix.API.Models.Requests;

public class SigningRoomCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? OriginalPath { get; set; }
    public int NotaryId { get; set; }
    public int CreatedBy { get; set; }
    public int StatusId { get; set; } = 1; // Default to active status
    public JsonElement MetaData { get; set; }
}
public class RegisterSigningRoomRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("originalPath")]
    public string? OriginalPath { get; set; }
    [JsonPropertyName("notaryEmail")]
    public string NotaryEmail { get; set; }
    [JsonPropertyName("documents")]
    public List<RegisterSigningRoomDocumentRequest> Documents { get; set; } = new();
    [JsonPropertyName("signers")]
    public List<RegisterSigningRoomSignerRequest> Signers { get; set; } = new();
    [JsonPropertyName("metaData")]
    public JsonElement MetaData { get; set; }
}
public class RegisterSigningRoomSignerRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    [JsonPropertyName("designation")]
    public Meta.DesignationEnum? Designation { get; set; }
}
public class RegisterSigningRoomDocumentRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("docTags")]
    public JsonElement DocTags { get; set; }
    [JsonPropertyName("fileType")]
    public string? FileType { get; set; }
    [JsonPropertyName("fileSize")]
    public long FileSize { get; set; }
}
public class SigningRoomUpdateRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? OriginalPath { get; set; }
    public string? SignedPath { get; set; }
    public int NotaryId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int ModifiedBy { get; set; }
    public int StatusId { get; set; }
    public JsonElement MetaData { get; set; }
}

public class SigningRoomQuery
{
    public int? NotaryId { get; set; }
    public int? StatusId { get; set; }
    public string? Name { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}