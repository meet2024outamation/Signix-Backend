using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;
using Signix.IAM.API.Infrastructure.Utility;

namespace Signix.IAM.API.Endpoints.Client
{
    [Route("api/client")]
    public class GetAccessibleClient : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<List<GetAccessibleClientResponse>>
    {
        private readonly IMemCacheService _memCacheServices;
        public GetAccessibleClient(IMemCacheService memCacheServices)
        {
            _memCacheServices = memCacheServices;
        }
        [HttpGet("accessible")]
        [SwaggerOperation(Summary = "Accessible Client", Description = "", OperationId = "Client.GetAccessible", Tags = new[] { "Client" }
      )]
        public override async Task<ActionResult<List<GetAccessibleClientResponse>>> HandleAsync(CancellationToken cancellationToken = default)
        {
            var result = await _memCacheServices.GetAccessibleClientAsync(User.FindFirst(AppAMUser.GetUniqueIdentityParameter(User.Claims))?.Value!);
            return result.ToActionResult(this);
        }
    }
}
