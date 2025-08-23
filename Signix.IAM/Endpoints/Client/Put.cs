using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;

namespace Signix.IAM.API.Endpoints.Client
{
    [Route("api/client")]
    public class Put : EndpointBaseAsync
   .WithRequest<ClientEditRequest>
   .WithActionResult<string>
    {
        private readonly IClientServices _clientServices;
        public Put(IClientServices clientServices)
        {
            _clientServices = clientServices;
        }
        [HttpPut("edit")]
        [SwaggerOperation(Summary = "Edit Client", Description = "", OperationId = "Client.Edit", Tags = new[] { "Client" }
      )]
        public override async Task<ActionResult<string>> HandleAsync([FromBody] ClientEditRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _clientServices.UpdateClientAsync(request);
            return result.ToActionResult(this);
        }
    }
}
