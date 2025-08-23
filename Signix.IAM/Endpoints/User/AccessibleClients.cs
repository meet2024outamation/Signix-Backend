using Ardalis.ApiEndpoints;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;
using Signix.IAM.API.Infrastructure.Utility;
using Signix.IAM.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Signix.IAM.API.Endpoints.User
{
    [Authorize]
    [Route("api/user")]
    public class AccessibleClients : EndpointBaseAsync
     .WithoutRequest
     .WithActionResult<int>
    {
        private readonly IManageUserService _manageUserService;
        public AccessibleClients(IManageUserService manageUserService)
        {
            _manageUserService = manageUserService;
        }
        [HttpGet("accessible-client")]
        [SwaggerOperation(Summary = "accessible-client", Description = "", OperationId = "User.accessibleClient", Tags = new[] { "AccessibleClients" })]
        public override async Task<ActionResult<int>> HandleAsync(CancellationToken cancellationToken = default)
        {
            var result = await _manageUserService.GetUserAccessibleClientsById(User.FindFirst(AppAMUser.GetUniqueIdentityParameter(User.Claims))?.Value!);

            return result.ToActionResult(this);
        }
    }
}
