namespace Signix.API.Models.Requests;

public class DocumentQuery
{
    public int? SigningRoomId { get; set; }
    public int? ClientId { get; set; }
    public int? DocumentStatusId { get; set; }
    public string? Name { get; set; }
    public string? FileType { get; set; }
    public string? DocTags { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
public class SignDocumentRequest
{
    public int SignningRoomId { get; set; }
    public List<int> DocumentIds { get; set; }
}
