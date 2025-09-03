using Ardalis.Result;
using Signix.API.Models.Requests;
using Signix.API.Models.Responses;

namespace Signix.API.Infrastructure;

public interface IDocumentService
{
    Task<PagedResult<List<ListDocumentResponse>>> GetAllAsync(DocumentQuery query);
    Task<Result<int>> SignDocumentsAsync(SignDocumentRequest request);
}