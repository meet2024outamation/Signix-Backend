using Ardalis.ApiEndpoints;
using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using Signix.API.Extensions;
using Signix.API.Infrastructure;
using Signix.API.Models.Requests;
using Signix.Entities.Entities;

namespace Signix.API.Endpoints.SigningRooms;

public class Create : EndpointBaseAsync
    .WithRequest<CreateSigningRoomRequest>
    .WithActionResult
{
    private readonly ISigningRoomService _signingRoomService;

    public Create(ISigningRoomService signingRoomService)
    {
        _signingRoomService = signingRoomService;
    }

    [HttpPost("/api/signing-rooms")]
    public override async Task<ActionResult> HandleAsync(
        CreateSigningRoomRequest request,
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