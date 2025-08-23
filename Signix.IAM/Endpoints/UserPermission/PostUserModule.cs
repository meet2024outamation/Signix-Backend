using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;

namespace Signix.IAM.API.Endpoints.UserPermission
{
    [Authorize]
    [Route("api/users/{UserId}/modules/{ModuleId}")]
    public class PostUserModule : EndpointBaseAsync
    .WithRequest<UpdateUserModuleRequest>
    .WithActionResult<int>
    {
        private readonly IUserPermissionServices _userPermissionServices;
        public PostUserModule(IUserPermissionServices userPermissionServices)
        {
            _userPermissionServices = userPermissionServices;
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Add User Module", Description = "", OperationId = "UserModule.Create", Tags = new[] { "User Module" }
      )]
        public override async Task<ActionResult<int>> HandleAsync([FromRoute] UpdateUserModuleRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _userPermissionServices.PostUserModule(request);
            return result.ToActionResult(this);
        }
    }
}
