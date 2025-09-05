namespace Signix.API.Models.Responses;

public class SigningRoomListResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? OriginalPath { get; set; }
    public string? SignedPath { get; set; }
    public string NotaryName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int SignersCount { get; set; }
    public int DocumentsCount { get; set; }
    //public Dictionary<string, object>? MetaData { get; set; }
    //public Dictionary<string, object>? SignTags { get; set; }
}
public class SigningRoomGetByIdResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? OriginalPath { get; set; }
    public string? SignedPath { get; set; }
    public int NotaryId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int ModifiedBy { get; set; }
    public List<ListSignignRoomsSignerResponse> Signers { get; set; }
    public List<GetByIdSignignRoomDocumentResponse> Documents { get; set; }
    //public Dictionary<string, object>? MetaData { get; set; }
    //public Dictionary<string, object>? SignTags { get; set; }
}
public class ListSignignRoomsSignerResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public string Email { get; set; }
}
public class ListSignignRoomDocumentResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
}

public class GetByIdSignignRoomDocumentResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public long FileSize { get; set; }
    public string FileType { get; set; }
}