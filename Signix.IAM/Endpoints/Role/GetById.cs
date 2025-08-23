using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Signix.IAM.API.Endpoints.Permission;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;

namespace Signix.IAM.API.Endpoints.Role
{
    
    [Route("api/roles")]
    public class GetById : EndpointBaseAsync
    .WithRequest<int>
    .WithActionResult<GetRoleResponse>
    {
        private readonly IRoleServices _roleServices;
        public GetById(IRoleServices roleServices)
        {
            _roleServices = roleServices;
        }
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get By Id", Description = "", OperationId = "Role.Id", Tags = new[] { "Role" }
      )]
        public override async Task<ActionResult<GetRoleResponse>> HandleAsync(int id, CancellationToken cancellationToken = default)
        {
            var result = await _roleServices.GetRoleByIdAsync(id);
            return result.ToActionResult(this);
        }
    }
}
