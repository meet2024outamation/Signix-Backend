using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;
using Signix.IAM.API.Models;
using Signix.IAM.API.Models;

namespace Signix.IAM.API.Endpoints.Client
{
    [Route("api/client")]
    public class Post : EndpointBaseAsync
   .WithRequest<ClientCreateRequest>
   .WithActionResult<string>
    {
        private readonly IClientServices _clientServices;
        public Post(IClientServices clientServices)
        {
            _clientServices = clientServices;
        }
        [HttpPost("create")]
        [SwaggerOperation(Summary = "Create Client", Description = "", OperationId = "Client.Create", Tags = new[] { "Client" }
      )]
        public override async Task<ActionResult<string>> HandleAsync([FromBody] ClientCreateRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _clientServices.CreateClientAsync(request);
            return result.ToActionResult(this);
        }
    }
}
