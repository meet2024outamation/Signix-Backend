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
    [Route("api/user")]
    [Authorize]
    public class Me : EndpointBaseAsync
     .WithoutRequest
     .WithActionResult<UserDetail?>
    {
        private readonly IManageUserService _manageUserService;
        public Me(IManageUserService manageUserService)
        {
            _manageUserService=manageUserService;
        }
        [HttpGet("me")]
        [SwaggerOperation(Summary = "Me", Description = "", OperationId = "User.Me", Tags = new[] { "Me" }
      )]
        public override async Task<ActionResult<UserDetail?>> HandleAsync(CancellationToken cancellationToken = default)
        {
            var result = await _manageUserService.GetUserDetailById(Request.Headers["Client-Id"].ToString(), User.FindFirst(AppAMUser.GetUniqueIdentityParameter(User.Claims))?.Value!);
            
            return result.ToActionResult(this);
        }
    }
}
