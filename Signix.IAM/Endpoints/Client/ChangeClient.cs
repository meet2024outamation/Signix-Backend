using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;
using Signix.IAM.API.Infrastructure.Utility;

namespace Signix.IAM.API.Endpoints.Client
{
    [Route("api/Client")]
    public class ChangeClient : EndpointBaseAsync
   .WithRequest<ChangeClientRequest>
   .WithActionResult<int>
    {
        private readonly IClientServices _ClientServices;
        public ChangeClient(IClientServices ClientServices)
        {
            _ClientServices = ClientServices;
        }
        [HttpPost("change")]
        [SwaggerOperation(Summary = "Change User Client", Description = "", OperationId = "Client.Change", Tags = new[] { "Client" }
      )]
        public override async Task<ActionResult<int>> HandleAsync([FromBody] ChangeClientRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _ClientServices.ChangeCurrentClientAsync(request, User.FindFirst(AppAMUser.GetUniqueIdentityParameter(User.Claims))?.Value!);
            return result.ToActionResult(this);
        }
    }
}
