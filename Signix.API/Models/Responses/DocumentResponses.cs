namespace Signix.API.Models.Responses;

public class ListDocumentResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SigningRoom { get; set; } = string.Empty;
    public string OriginalPath { get; set; } = string.Empty;
    public string? SignedPath { get; set; }
}
