using Ardalis.ApiEndpoints;
using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using Signix.API.Extensions;
using Signix.API.Infrastructure;
using Signix.API.Models.Requests;
using Signix.Entities.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Signix.API.Endpoints.Signers;

public class UpdateById : EndpointBaseAsync
    .WithRequest<UpdateSignerByIdEndpointRequest>
    .WithActionResult<Signer>
{
    private readonly ISignerService _signerService;

    public UpdateById(ISignerService signerService)
    {
        _signerService = signerService;
    }

    [HttpPut("/api/signers/{signerId}/signature")]
    [SwaggerOperation(
        Summary = "Update Signer Signature",
        Description = "Updates the signature data for a specific signer with base64 encoded signature",
        OperationId = "UpdateSigner.Signature",
        Tags = new[] { "Signers" }
    )]
    public override async Task<ActionResult<Signer>> HandleAsync(
        [FromRoute] UpdateSignerByIdEndpointRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Base64Signature))
        {
            var validationError = new ValidationError
            {
                Identifier = nameof(request.Base64Signature),
                ErrorMessage = "Base64 signature data is required"
            };
            return Result<Signer>.Invalid(validationError).ToActionResult();
        }

        // Update signature
        var result = await _signerService.UpdateSignatureAsync(request);
        return result.ToActionResult();
    }
}
