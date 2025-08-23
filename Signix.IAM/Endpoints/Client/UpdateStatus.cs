using Ardalis.ApiEndpoints;
using Signix.IAM.API.Models;
using Signix.IAM.API.Endpoints.Department;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Signix.IAM.API.Endpoints.Client
{
    public class UpdateStatusById : EndpointBaseAsync.WithRequest<UpdateStatusByIdR>.WithActionResult<string>
    {
        private readonly IClientServices _clientServices;

        public UpdateStatusById(IClientServices clientServices)
        {
            _clientServices = clientServices;
        }
        [HttpPatch("/api/client/{id}")]
        [SwaggerOperation(
            Description = "Update Client Status",
            OperationId = "Clients.Status",
            Tags = new[] { "Client" }
            )]
        public override async Task<ActionResult<string>> HandleAsync(UpdateStatusByIdR request, CancellationToken cancellationToken = default)
        {
            var result = await _clientServices.UpdateStatusByIdAsync(request);
            return result.ToActionResult(this);
        }
    }
}
