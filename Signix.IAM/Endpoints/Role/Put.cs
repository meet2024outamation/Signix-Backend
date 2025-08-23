using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;

namespace Signix.IAM.API.Endpoints.Role
{
    [Route("api/role")]
    public class Put : EndpointBaseAsync
    .WithRequest<RoleUpdateRequest>
    .WithActionResult<int>
    {
        private readonly IRoleServices _roleServices;
        public Put(IRoleServices roleServices)
        {
            _roleServices = roleServices;
        }
        [HttpPut]
        [SwaggerOperation(Summary = "Update Role", Description = "", OperationId = "Role.Update", Tags = new[] { "Role" }
      )]
        public override async Task<ActionResult<int>> HandleAsync([FromBody] RoleUpdateRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _roleServices.UpdateAsync(request);
            return result.ToActionResult(this);
        }
    }
}
