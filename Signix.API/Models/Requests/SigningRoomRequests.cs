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
    public Dictionary<string, object>? MetaData { get; set; }
    public Dictionary<string, object>? SignTags { get; set; }
}

public class RegisterSigningRoomRequest
{
    [JsonPropertyName("client")]
    public string Client { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("originalPath")]
    public string? OriginalPath { get; set; }

    [JsonPropertyName("notaryEmail")]
    public string NotaryEmail { get; set; } = string.Empty;

    [JsonPropertyName("signTags")]
    public Dictionary<string, object>? SignTags { get; set; }
    [JsonPropertyName("documents")]
    public List<RegisterSigningRoomDocumentRequest> Documents { get; set; } = new();

    [JsonPropertyName("signers")]
    public List<RegisterSigningRoomSignerRequest> Signers { get; set; } = new();

    [JsonPropertyName("metaData")]
    public Dictionary<string, object>? MetaData { get; set; }
}

public class RegisterSigningRoomSignerRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Designation name (e.g., "Borrower", "Loan Officer", "Notary Public", "Manager", "CEO")
    /// Will be mapped to the corresponding designation ID from the designations table
    /// </summary>
    [JsonPropertyName("designation")]
    public string Designation { get; set; } = string.Empty;
}

public class RegisterSigningRoomDocumentRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Document tags as Dictionary for flexible key-value pairs
    /// Example: {"borr_name": "John Doe", "loan_amount": 50000, "credit_score": 750}
    /// </summary>
    [JsonPropertyName("docTags")]
    public Dictionary<string, object> DocTags { get; set; } = new();

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
    public Dictionary<string, object>? MetaData { get; set; }
    public Dictionary<string, object>? SignTags { get; set; }
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