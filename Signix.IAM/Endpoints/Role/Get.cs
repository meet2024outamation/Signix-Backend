using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;

namespace Signix.IAM.API.Endpoints.Role
{
    [Route("api/roles")]
    public class Get : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<List<GetRoleResponse>>
    {
        private readonly IRoleServices _roleServices;
        public Get(IRoleServices roleServices)
        {
            _roleServices = roleServices;
        }
        [HttpGet]
        [SwaggerOperation(Summary = "Get All Role", Description = "", OperationId = "Role.All", Tags = new[] { "Role" }
      )]
        public override async Task<ActionResult<List<GetRoleResponse>>> HandleAsync(CancellationToken cancellationToken = default)
        {
            var result = await _roleServices.GetRoleAsync();
            return result.ToActionResult(this);
        }
    }
}
