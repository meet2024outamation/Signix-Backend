using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;

namespace Signix.IAM.API.Endpoints.Role
{
    [Route("api/role")]
    public class GetRoleUser : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<GetRoleUserResponse>
    {
        private readonly IRoleServices _roleServices;
        public GetRoleUser(IRoleServices roleServices)
        {
            _roleServices = roleServices;
        }
        [HttpGet("users")]
        [SwaggerOperation(Summary = "Get Role Users By Id", Description = "", OperationId = "RoleUser.Id", Tags = new[] { "Role" }
      )]
        public override async Task<ActionResult<GetRoleUserResponse>> HandleAsync(CancellationToken cancellationToken = default)
        {
            var result = await _roleServices.GetRoleUsersAsync();
            return result.ToActionResult(this);
        }
    }
}
