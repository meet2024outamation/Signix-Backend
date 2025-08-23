using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;

namespace Signix.IAM.API.Endpoints.Permission
{
    [Route("api/permission")]
    public class Get : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<List<GetPermissionResponse>>
    {
        private readonly IPermissionServices _permissionServices;
        public Get(IPermissionServices permissionServices)
        {
            _permissionServices = permissionServices;
        }
        [HttpGet]
        [SwaggerOperation(Summary = "Get All Permission", Description = "", OperationId = "Permission.All", Tags = new[] { "Permission" }
      )]
        public override async Task<ActionResult<List<GetPermissionResponse>>> HandleAsync(CancellationToken cancellationToken = default)
        {
            var result = await _permissionServices.GetPermissionAsync();
            return result.ToActionResult(this);
        }
    }
}
