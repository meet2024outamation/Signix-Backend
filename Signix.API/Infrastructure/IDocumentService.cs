using Ardalis.Result;
using Signix.API.Models.Requests;
using Signix.Entities.Entities;

namespace Signix.API.Infrastructure;

public interface IDocumentService
{
    Task<PagedResult<List<Document>>> GetAllAsync(DocumentQuery query);
    Task<Result<int>> SignDocumentsAsync(SignDocumentRequest request);
}