using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;

namespace Signix.IAM.API.Endpoints.Client
{
    [Route("api/Client")]
    public class Get : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<List<ClientGetResponse>>
    {
        private readonly IClientServices _ClientServices;
        public Get(IClientServices ClientServices)
        {
            _ClientServices = ClientServices;
        }
        [HttpGet("all")]
        [SwaggerOperation(Summary = "Create Client", Description = "", OperationId = "Client.All", Tags = new[] { "Client" }
      )]
        public override async Task<ActionResult<List<ClientGetResponse>>> HandleAsync(CancellationToken cancellationToken = default)
        {
            var result = await _ClientServices.GetClientAsync();
            return result.ToActionResult(this);
        }
    }
}
