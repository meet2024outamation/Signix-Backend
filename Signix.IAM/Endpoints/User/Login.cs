using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;
using Signix.IAM.API.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Signix.IAM.API.Endpoints.User
{
    [Route("api/user")]
    public class Login : EndpointBaseAsync
     .WithoutRequest
     .WithActionResult<int>
    {
        private readonly IManageUserService _manageUserService;
        public Login(IManageUserService manageUserService)
        {
            _manageUserService = manageUserService;
        }
        [HttpGet("login")]
        [SwaggerOperation(Summary = "Login", Description = "", OperationId = "User.Login", Tags = new[] { "Me" })]
        public override async Task<ActionResult<int>> HandleAsync(CancellationToken cancellationToken = default)
        {            
            var result = await _manageUserService.LoginUser(Request.Headers["Authorization"].First()!.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]);
            return result.ToActionResult(this);
        }
    }
}
