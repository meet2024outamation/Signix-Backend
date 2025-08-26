using Ardalis.ApiEndpoints;
using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using Signix.API.Extensions;
using Signix.API.Infrastructure;
using Signix.API.Models.Requests;
using Signix.Entities.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Signix.API.Endpoints.SigningRooms;

public class Update : EndpointBaseAsync
    .WithRequest<UpdateSigningRoomEndpointRequest>
    .WithActionResult
{
    private readonly ISigningRoomService _signingRoomService;

    public Update(ISigningRoomService signingRoomService)
    {
        _signingRoomService = signingRoomService;
    }

    [HttpPut("/api/signing-rooms/{id}")]
    [SwaggerOperation(
      Summary = "Update Signing Room",
      Description = "",
      OperationId = "Update.SigningRoom",
      Tags = new[] { "Signing Room" }
     )]
    public override async Task<ActionResult> HandleAsync(
        UpdateSigningRoomEndpointRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Body.Name))
        {
            var validationError = new ValidationError
            {
                Identifier = nameof(request.Body.Name),
                ErrorMessage = "Name is required"
            };
            return Result<SigningRoom>.Invalid(validationError).ToActionResult();
        }

        var updateRequest = new SigningRoomUpdateRequest
        {
            Id = request.Id,
            Name = request.Body.Name,
            Description = request.Body.Description,
            OriginalPath = request.Body.OriginalPath,
            SignedPath = request.Body.SignedPath,
            NotaryId = request.Body.NotaryId,
            StartedAt = request.Body.StartedAt,
            CompletedAt = request.Body.CompletedAt,
            ModifiedBy = request.Body.ModifiedBy,
            StatusId = request.Body.StatusId,
            MetaData = request.Body.MetaData
        };

        var result = await _signingRoomService.UpdateAsync(updateRequest);
        return result.ToActionResult();
    }
}

public class UpdateSigningRoomEndpointRequest
{
    [FromRoute]
    public int Id { get; set; }

    [FromBody]
    public UpdateSigningRoomBody Body { get; set; } = new();
}

public class UpdateSigningRoomBody
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? OriginalPath { get; set; }
    public string? SignedPath { get; set; }
    public int NotaryId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int ModifiedBy { get; set; }
    public int StatusId { get; set; }
    public System.Text.Json.JsonElement MetaData { get; set; }
}