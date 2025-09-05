using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Signix.API.Extensions;
using Signix.API.Infrastructure;
using Signix.API.Models.Requests;
using Swashbuckle.AspNetCore.Annotations;

namespace Signix.API.Endpoints.Documents;

public class List : EndpointBaseAsync
    .WithRequest<DocumentQuery>
    .WithActionResult
{
    private readonly IDocumentService _documentService;

    public List(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet("/api/documents")]
    [SwaggerOperation(
        Summary = "List Documents",
        Description = "Retrieve a paginated list of documents with optional filtering by signing room ID and other parameters",
        OperationId = "List.Documents",
        Tags = new[] { "Documents" }
    )]
    public override async Task<ActionResult> HandleAsync(
        [FromQuery] DocumentQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await _documentService.GetAllAsync(request);
        return result.ToActionResult();
    }
}