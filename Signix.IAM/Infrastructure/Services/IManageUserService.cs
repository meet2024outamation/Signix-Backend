using SharedKernel.Result;
using Signix.IAM.API.Models;
using Signix.IAM.Entities;
using SharedKernel.Services;

namespace Signix.IAM.API.Infrastructure.Services
{
    public interface IManageUserService
    {
        public Task<IEnumerable<UserList>> GetUsers(bool? isActive, string? currentClientId);
        public Task<Result<UserEM>> GetUserById(int id);
        public Task<Result<UserVM>> GetUserByUniqueId(string uniqueId);
        public Task<Result<UserEM>> GetUserByEmail(string email);
        public Task<Result<User>> CreateUser(UserCM user, bool isClientAdmin = false);
        public Task<Result<User>> UpdateUser(UserEM user);
        public Task<Result<User>> DisableUser(string uniqueId, bool accountEnabled);
        public Task<Result<User>> DeleteUser(string uniqueId);
        public Task<Result<List<UserBasicInfo>>> GetActiveUsersByRolesAsync(string role);
        public Task<Result<List<UserBasicInfo>>> GetActiveUsersByPermissionAsync(string permission, string currentClientId);
        public Task<Result<bool>> IsEmailExists(string email, int userId);
        public Task<Result<UserDetail?>> GetUserDetailById(string clientId, string id);
        public Task<IList<string>> GetEmails();
        public Task<Result<int>> LoginUser(string accessToken);
        public Task<Result<int>> LogoutUser(string accessToken);
        public Task<Result<List<AccessibleClientDTO>?>> GetUserAccessibleClientsById(string id);
    }
}
