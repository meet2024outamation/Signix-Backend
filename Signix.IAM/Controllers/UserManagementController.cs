using Microsoft.AspNetCore.Mvc;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;
using Signix.IAM.API.Infrastructure.Utility;
using Signix.IAM.API.Models;

namespace Signix.IAM.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly IManageUserService _manageUserService;
        private readonly ILogger<UserManagementController> _logger;
        private readonly IMemCacheService _memCache;

        public UserManagementController(IManageUserService manageUserService, ILogger<UserManagementController> logger, IMemCacheService memCache)
        {
            _manageUserService = manageUserService;
            _logger = logger;
            _memCache = memCache;
        }
        /// <summary>
        /// Get Users
        /// </summary>
        /// <returns></returns>
        //[HasPermission(Permissions.ManageUsers.ViewUser)]
        [HttpPost("users")]
        public async Task<IEnumerable<UserList>> GetAsync([FromBody] UserParam userParam)
        {
            return await _manageUserService.GetUsers(userParam.IsActive, userParam.CurrentClientId);
        }


        /// <summary>
        /// Get User by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetAsync(string id)
        {
            var result = await _manageUserService.GetUserByUniqueId(id);
            return result.ToActionResult(this);
        }

        [HttpGet("user-detail/{clientId}")]
        public async Task<IActionResult> GetUserAsync([FromRoute] string clientId)
        {
            return Ok(await _memCache.GetUserById(User.FindFirst(AppAMUser.GetUniqueIdentityParameter(User.Claims))?.Value!, clientId));
        }

        [HttpGet("activeusersbyrole")]
        public async Task<IActionResult> GetActiveUsersByRolesAsync(string role)
        {
            var result = await _manageUserService.GetActiveUsersByRolesAsync(role);
            return result.ToActionResult(this);
        }

        [HttpGet("activeusersbypermission")]
        public async Task<IActionResult> GetActiveUsersByPermissionAsync(string permission, string currentClientId)
        {
            var result = await _manageUserService.GetActiveUsersByPermissionAsync(permission, currentClientId);
            return result.ToActionResult(this);
        }

        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("create-user")]
        public async Task<IActionResult> PostAsync(UserCM userCM)
        {
            var result = await _manageUserService.CreateUser(userCM);
            return result.ToActionResult(this);
        }

        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPut("update-user")]
        public async Task<IActionResult> PutAsync(UserEM userEM)
        {
            var result = await _manageUserService.UpdateUser(userEM);
            return result.ToActionResult(this);
        }

        /// <summary>
        /// Disabled user 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPatch("update-user-status")]
        public async Task<IActionResult> PatchAsync(string id, bool accountEnabled)
        {
            var result = await _manageUserService.DisableUser(id, accountEnabled);
            return result.ToActionResult(this);
        }

        /// <summary>
        /// Delete user 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpDelete("delete-user")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            var result = await _manageUserService.DeleteUser(id);
            return result.ToActionResult(this);
        }

        /// <summary>
        /// Check email 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet("IsEmailExists/{email}")]
        public async Task<IActionResult> IsEmailExists(string email)
        {
            var user = await _manageUserService.GetUserByEmail(User.FindFirst(AppAMUser.GetUniqueIdentityParameter(User.Claims))?.Value!);
            var result = await _manageUserService.IsEmailExists(email, user.Value!.Id!.Value);
            return result.ToActionResult(this);
        }

    }
}
