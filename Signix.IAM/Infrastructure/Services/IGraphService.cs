
using Microsoft.Graph.Models;

namespace Signix.IAM.API.Infrastructure.Services
{
    public interface IGraphService
    {
        public Task<SignInCollectionResponse?> GetSignInsLogs(string signInEventType);
        public Task<Invitation?> InviteGuestUser(Entities.User user);
        public Task<AppRoleAssignment?> UserAssignment(string userId);
        public Task UpdateUser(Entities.User user);
        public Task DeleteUserByAzureADId(string azureAdId);
        public Task DisabledUser(string azureADUserId, bool accountEnabled);
        public Task<User?> GetUserByEmail(string email);
    }
}
