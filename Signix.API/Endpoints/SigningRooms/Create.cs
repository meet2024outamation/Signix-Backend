using Ardalis.ApiEndpoints;
using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using Signix.API.Extensions;
using Signix.API.Infrastructure;
using Signix.API.Models.Requests;
using Signix.Entities.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Signix.API.Endpoints.SigningRooms;

public class Create : EndpointBaseAsync
    .WithRequest<SigningRoomCreateRequest>
    .WithActionResult
{
    private readonly ISigningRoomService _signingRoomService;

    public Create(ISigningRoomService signingRoomService)
    {
        _signingRoomService = signingRoomService;
    }

    [HttpPost("/api/signing-room")]
    [SwaggerOperation(
      Summary = "Create Signing Room",
      Description = "",
      OperationId = "Create.SigningRoom",
      Tags = new[] { "Signing Room" }
      )]
    public override async Task<ActionResult> HandleAsync(
        SigningRoomCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            var validationError = new ValidationError
            {
                Identifier = nameof(request.Name),
                ErrorMessage = "Name is required"
            };
            return Result<SigningRoom>.Invalid(validationError).ToActionResult();
        }

        var result = await _signingRoomService.CreateAsync(request);
        return result.ToActionResult();
    }
}