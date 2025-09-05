using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Signix.API.Extensions;
using Signix.API.Infrastructure;
using Signix.API.Models.Requests;
using Swashbuckle.AspNetCore.Annotations;

namespace Signix.API.Endpoints.Documents;

public class SignDocument : EndpointBaseAsync
    .WithRequest<SignDocumentRequest>
    .WithActionResult
{
    private readonly IDocumentService _documentService;

    public SignDocument(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost("/api/sign-documents")]
    [SwaggerOperation(
        Summary = "Sign Documents",
        Description = "Sign multiple documents in a signing room and publish event to RabbitMQ",
        OperationId = "Sign.Documents",
        Tags = new[] { "Documents" }
    )]
    [ProducesResponseType(typeof(int), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public override async Task<ActionResult> HandleAsync(
        SignDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _documentService.SignDocumentsAsync(request);
        return result.ToActionResult();
    }
}
