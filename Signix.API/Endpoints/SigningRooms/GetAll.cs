using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Signix.API.Extensions;
using Signix.API.Infrastructure;
using Signix.API.Models.Requests;

namespace Signix.API.Endpoints.SigningRooms;

public class GetAll : EndpointBaseAsync
    .WithRequest<SigningRoomQuery>
    .WithActionResult
{
    private readonly ISigningRoomService _signingRoomService;

    public GetAll(ISigningRoomService signingRoomService)
    {
        _signingRoomService = signingRoomService;
    }

    [HttpGet("/api/signing-rooms")]
    public override async Task<ActionResult> HandleAsync(
        [FromQuery] SigningRoomQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await _signingRoomService.GetAllAsync(request);
        return result.ToActionResult();
    }
}