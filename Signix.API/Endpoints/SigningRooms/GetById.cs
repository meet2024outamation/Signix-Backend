using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Signix.API.Extensions;
using Signix.API.Infrastructure;

namespace Signix.API.Endpoints.SigningRooms;

public class GetById : EndpointBaseAsync
    .WithRequest<int>
    .WithActionResult
{
    private readonly ISigningRoomService _signingRoomService;

    public GetById(ISigningRoomService signingRoomService)
    {
        _signingRoomService = signingRoomService;
    }

    [HttpGet("/api/signing-rooms/{id}")]
    public override async Task<ActionResult> HandleAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        var result = await _signingRoomService.GetByIdAsync(id);
        return result.ToActionResult();
    }
}