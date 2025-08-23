using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;

namespace Signix.IAM.API.Endpoints.Client
{
    [Route("api/client")]
    public class GetById : EndpointBaseAsync
    .WithRequest<string>
    .WithActionResult<List<ClientGetResponse>>
    {
        private readonly IClientServices _clientServices;
        public GetById(IClientServices clientServices)
        {
            _clientServices = clientServices;
        }
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get Client", Description = "", OperationId = "Client.GetId", Tags = new[] { "Client" }
      )]
        public override async Task<ActionResult<List<ClientGetResponse>>> HandleAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await _clientServices.GetClientByIdAsync(id);
            return result.ToActionResult(this);
        }
    }
}
