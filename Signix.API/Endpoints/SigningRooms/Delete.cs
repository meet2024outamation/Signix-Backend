using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Signix.API.Extensions;
using Signix.API.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;

namespace Signix.API.Endpoints.SigningRooms;

public class Delete : EndpointBaseAsync
    .WithRequest<int>
    .WithActionResult
{
    private readonly ISigningRoomService _signingRoomService;

    public Delete(ISigningRoomService signingRoomService)
    {
        _signingRoomService = signingRoomService;
    }

    [HttpDelete("/api/signing-rooms/{id}")]
    [SwaggerOperation(
      Summary = "Delete Signing Room",
      Description = "",
      OperationId = "Delete.SigningRoom",
      Tags = new[] { "Signing Room" }
      )]
    public override async Task<ActionResult> HandleAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        var result = await _signingRoomService.DeleteAsync(id);
        return result.ToActionResult();
    }
}