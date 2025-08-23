using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using SharedKernel;
using Signix.IAM.API.Models;

namespace Signix.IAM.API.Infrastructure.Services
{
    public class GraphService : IGraphService
    {
        private readonly AzureADConfig _azureADConfig;
        private readonly GraphServiceClient _graphServiceClient;
        public GraphService(IOptions<AzureADConfig> azureADConfig)
        {
            _azureADConfig = azureADConfig.Value;
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            };

            var requestAdapter = new ClientSecretCredential(
                _azureADConfig.TenantId, _azureADConfig.MasterClientId, _azureADConfig.MasterClientSecret, options);
            _graphServiceClient = new GraphServiceClient(requestAdapter);

        }
        public async Task<SignInCollectionResponse?> GetSignInsLogs(string signInEventType)
        {
            try
            {
                return await _graphServiceClient.AuditLogs.SignIns.GetAsync((requestConfiguration) =>
                    {
                        //requestConfiguration.QueryParameters.Top = 50;
                        requestConfiguration.QueryParameters.Filter = $"signInEventTypes/any(t:t eq '{signInEventType}')";
                    });
            }
            catch (ODataError odataError)
            {
                string guid = Guid.NewGuid().ToString();
                throw new Exception($"Error:{guid}:\n{odataError?.Error?.Code}\n{odataError?.Error?.Message}");
            }
        }
        /// <summary>
        /// Invite guest user
        /// API permissions in Azure App: User.Invite.All
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<Invitation?> InviteGuestUser(Entities.User user)
        {
            try
            {
                var invitation = new Invitation
                {
                    InviteRedirectUrl = _azureADConfig.InviteRedirectUrl,
                    InvitedUserEmailAddress = user.Email,
                    InvitedUserDisplayName = $"{user.FirstName} {user.LastName}",
                    InvitedUserMessageInfo = new InvitedUserMessageInfo
                    {
                        CustomizedMessageBody = _azureADConfig.InviteRedirectUrl
                    },
                    SendInvitationMessage = false
                };
                return await _graphServiceClient.Invitations.PostAsync(invitation);
            }
            catch (ODataError odataError)
            {
                string guid = Guid.NewGuid().ToString();
                throw new Exception($"Error:{guid}:\n{odataError?.Error?.Code}\n{odataError?.Error?.Message}");
            }
        }

        /// <summary>
        /// User assignment to app.
        /// API permissions  in Azure App: AppRoleAssignment.ReadWrite.All
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<AppRoleAssignment?> UserAssignment(string userId)
        {
            try
            {
                var servicePrincipal = await _graphServiceClient.ServicePrincipals.GetAsync((requestConfiguration) =>
                   {
                       requestConfiguration.QueryParameters.Filter = $"startswith(displayName, '{_azureADConfig.ClientAppName}')";
                   });

                var resource = servicePrincipal?.Value?.FirstOrDefault();
                if (resource == null || resource.Id.IsNullOrWhiteSpace())
                    return null;
                var alreadyAssigned = await _graphServiceClient.Users[userId]?.AppRoleAssignments.GetAsync((requestConfiguration) =>
                {
                    requestConfiguration.QueryParameters.Filter = $"resourceId eq {resource.Id}";
                });
                if (alreadyAssigned?.Value?.Count > 0 && !alreadyAssigned.Value[0].Id.IsNullOrWhiteSpace())
                {
                    return alreadyAssigned.Value[0];
                }
                var requestBody = new AppRoleAssignment
                {
                    PrincipalId = Guid.Parse(userId),//UserId
                    ResourceId = Guid.Parse(resource.Id),//ApplicationId
                    AppRoleId = Guid.Parse("00000000-0000-0000-0000-000000000000"),//RoleId
                };
                return await _graphServiceClient.Users[userId].AppRoleAssignments.PostAsync(requestBody);
            }
            catch (ODataError odataError)
            {
                string guid = Guid.NewGuid().ToString();
                throw new Exception($"Error:{guid}:\n{odataError?.Error?.Code}\n{odataError?.Error?.Message}");
            }
        }

        /// <summary>
        /// Update user data in Azure AD.
        /// API permissions in Azure App:User.ManageIdentities.All, User.EnableDisableAccount.All, User.ReadWrite.All, Directory.ReadWrite.All
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task UpdateUser(Entities.User user)
        {
            try
            {
                User updateUser = new()
                {
                    GivenName = user.FirstName,
                    Surname = user.LastName,
                };

                //if (!user.PhoneNumber.IsNullOrWhiteSpace())
                //    updateUser.MobilePhone = user.PhoneNumber;
                if (user.Id != 0)
                    updateUser.OnPremisesExtensionAttributes = new OnPremisesExtensionAttributes
                    {
                        ExtensionAttribute1 = user.UniqueId,
                    };
                await _graphServiceClient.Users[user.AzureAduserId].PatchAsync(updateUser);
            }
            catch (ODataError odataError)
            {
                string guid = Guid.NewGuid().ToString();
                throw new Exception($"Error:{guid}:\n{odataError?.Error?.Code}\n{odataError?.Error?.Message}");
            }
        }

        /// <summary>
        /// Disable User in Azure AD.
        /// API permissions in Azure App: User.EnableDisableAccount.All
        /// </summary>
        /// <param name="azureADUserId"></param>
        /// <param name="accountEnabled"></param>
        /// <returns></returns>
        public async Task DisabledUser(string azureADUserId, bool accountEnabled)
        {
            try
            {
                await _graphServiceClient.Users[azureADUserId].PatchAsync(new User
                {
                    AccountEnabled = accountEnabled,
                });
            }
            catch (ODataError odataError)
            {
                string guid = Guid.NewGuid().ToString();
                throw new Exception($"Error:{guid}:\n{odataError?.Error?.Code}\n{odataError?.Error?.Message}");
            }
        }

        /// <summary>
        /// Delete User in Azure AD.
        /// </summary>
        /// <param name="azureAdId"></param>
        /// <returns></returns>
        public async Task DeleteUserByAzureADId(string azureAdId)
        {
            try
            {
                await _graphServiceClient.Users[azureAdId].DeleteAsync();
            }
            catch (ODataError odataError)
            {
                string guid = Guid.NewGuid().ToString();
                throw new Exception($"Error:{guid}:\n{odataError?.Error?.Code}\n{odataError?.Error?.Message}");
            }
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            try
            {
                var user = await _graphServiceClient.Users.GetAsync((requestConfiguration) =>
                    {
                        requestConfiguration.QueryParameters.Filter = $"mail eq '{email}'";
                    });

                if (user == null || user.Value == null || user.Value.SingleOrDefault() == null)
                {
                    return null;
                }

                return user.Value.SingleOrDefault();
            }
            catch (ODataError odataError)
            {
                string guid = Guid.NewGuid().ToString();
                throw new Exception($"Error:{guid}:\n{odataError?.Error?.Code}\n{odataError?.Error?.Message}");
            }
        }
    }
}
