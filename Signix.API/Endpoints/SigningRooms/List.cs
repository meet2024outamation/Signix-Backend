using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Signix.API.Extensions;
using Signix.API.Infrastructure;
using Signix.API.Models.Requests;
using Swashbuckle.AspNetCore.Annotations;

namespace Signix.API.Endpoints.SigningRooms;

public class List : EndpointBaseAsync
    .WithRequest<SigningRoomQuery>
    .WithActionResult
{
    private readonly ISigningRoomService _signingRoomService;

    public List(ISigningRoomService signingRoomService)
    {
        _signingRoomService = signingRoomService;
    }

    [HttpGet("/api/signing-rooms")]
    [SwaggerOperation(
      Summary = "List Signing Room",
      Description = "",
      //OperationId = "List.SigningRoom",
      Tags = ["Signing Room"]
     )]
    public override async Task<ActionResult> HandleAsync(
        [FromQuery] SigningRoomQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await _signingRoomService.GetAllAsync(request);
        return result.ToActionResult();
    }
}