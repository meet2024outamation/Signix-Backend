using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;

namespace Signix.IAM.API.Endpoints.Permission
{

    [Route("api/permission")]
    public class GetById : EndpointBaseAsync
    .WithRequest<int>
    .WithActionResult<GetPermissionResponse>
    {
        private readonly IPermissionServices _permissionServices;
        public GetById(IPermissionServices permissionServices)
        {
            _permissionServices = permissionServices;
        }
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get By Id", Description = "", OperationId = "Permission.Id", Tags = new[] { "Permission" }
      )]
        public override async Task<ActionResult<GetPermissionResponse>> HandleAsync(int id, CancellationToken cancellationToken = default)
        {
            var result = await _permissionServices.GetPermissionByIdAsync(id);
            return result.ToActionResult(this);
        }
    }
}
