using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Signix.API.Extensions;
using Signix.API.Infrastructure;
using Signix.API.Models.Requests;
using Swashbuckle.AspNetCore.Annotations;

namespace Signix.API.Endpoints.SigningRooms
{
    public class Register : EndpointBaseAsync.WithRequest<RegisterSigningRoomRequest>.WithoutResult
    {
        private readonly ISigningRoomService _signingRoomService;

        public Register(ISigningRoomService signingRoomService)
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
            RegisterSigningRoomRequest request,
            CancellationToken cancellationToken = default)
        {
            //if (string.IsNullOrWhiteSpace(request.Name))
            //{
            //    var validationError = new ValidationError
            //    {
            //        Identifier = nameof(request.Name),
            //        ErrorMessage = "Name is required"
            //    };
            //    return Result<int>.Invalid(validationError).ToActionResult();
            //}

            var result = await _signingRoomService.RegisterAsync(request);
            return result.ToActionResult();
        }
    }
}
