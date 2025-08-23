using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Signix.IAM.API.Endpoints.Role;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;

namespace Signix.IAM.API.Endpoints.Role
{
    [Route("api/roles")]
    public class Post : EndpointBaseAsync
    .WithRequest<RoleCreateRequest>
    .WithActionResult<int>
    {
        private readonly IRoleServices _roleServices;
        public Post(IRoleServices roleServices)
        {
            _roleServices = roleServices;
        }
        [HttpPost]
        [SwaggerOperation(Summary = "Create Role", Description = "", OperationId = "Role.Create", Tags = new[] { "Role" }
      )]
        public override async Task<ActionResult<int>> HandleAsync([FromBody] RoleCreateRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _roleServices.CreateAsync(request);
            return result.ToActionResult(this);
        }
    }
}
