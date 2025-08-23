using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;

namespace Signix.IAM.API.Endpoints.UserPermission
{
    [Route("api/users/{UserId}/modules/{ModuleId}")]
    public class DeleteUserModule : EndpointBaseAsync
    .WithRequest<UpdateUserModuleRequest>
    .WithActionResult<int>
    {
        private readonly IUserPermissionServices _userPermissionServices;
        public DeleteUserModule(IUserPermissionServices userPermissionServices)
        {
            _userPermissionServices = userPermissionServices;
        }
        [HttpDelete]
        [SwaggerOperation(Summary = "Delete User Module", Description = "", OperationId = "UserModule.Delete", Tags = new[] { "User Module" }
      )]
        public override async Task<ActionResult<int>> HandleAsync([FromRoute] UpdateUserModuleRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _userPermissionServices.DeleteUserModule(request);
            return result.ToActionResult(this);
        }
    }
}
