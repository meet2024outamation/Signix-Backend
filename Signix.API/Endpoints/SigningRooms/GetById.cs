using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Signix.API.Extensions;
using Signix.API.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;

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
    [SwaggerOperation(
      Summary = "GetById Signing Room",
      Description = "",
      //OperationId = "GetById.SigningRoom",
      Tags = ["Signing Room"]
      )]
    public override async Task<ActionResult> HandleAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        var result = await _signingRoomService.GetByIdAsync(id);
        return result.ToActionResult();
    }
}