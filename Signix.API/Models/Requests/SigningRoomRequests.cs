using System.Text.Json;

namespace Signix.API.Models.Requests;

public class CreateSigningRoomRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? OriginalPath { get; set; }
    public int NotaryId { get; set; }
    public int CreatedBy { get; set; }
    public int StatusId { get; set; } = 1; // Default to active status
    public JsonElement MetaData { get; set; }
}

public class UpdateSigningRoomRequest
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