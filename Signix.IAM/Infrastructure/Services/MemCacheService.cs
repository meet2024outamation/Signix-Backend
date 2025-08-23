using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Office2010.Excel;
using Fluid;
using Signix.IAM.API.Endpoints.Client;
using Signix.IAM.API.Models;
using Signix.IAM.Context;
using Signix.IAM.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SharedKernel;
using SharedKernel.Adapters;
using SharedKernel.AuthorizeHandler;
using SharedKernel.Models;
using SharedKernel.Result;
using SharedKernel.Services;
using SharedKernel.Utility;
using System.Text;
using System.Text.Json;
using static IAM.API.Infrastructure.Utility.Utility;

namespace Signix.IAM.API.Infrastructure.Services
{
    public class MemCacheService : IMemCacheService
    {
        private readonly IDistributedCache _memoryCache;
        private readonly IMapper _mapper;
        private readonly IAMDbContext _iamDbConext;

        public MemCacheService(IAMDbContext iamDbConext, IDistributedCache memoryCache, IMapper mapper)
        {
            _memoryCache = memoryCache;
            _mapper = mapper;
            _iamDbConext = iamDbConext;
        }
        public EmailTemplate? InviteUser => GetFluidTemplate(nameof(InviteUser));
        public EmailTemplate? ClientAssignment => GetFluidTemplate(nameof(ClientAssignment));

        public async Task<UserWithClients> GetUserById(string uniqueId,string clientId)
        {

            var userDetail = await _memoryCache.GetAsync<UserWithClients>($"{uniqueId}-{clientId}");
            if (userDetail != null)
            {
                return userDetail;
            }
            // var user = await _iamDbConext.Users.ProjectTo<UserWithClients>(_mapper.ConfigurationProvider).SingleOrDefaultAsync(s => s.Email.ToLower() == uniqueId.ToLower());

            var user = await _iamDbConext.Users
           .Where(u => u.Email.ToLower() == uniqueId.ToLower())
           .Select(u => new UserWithClients
           {
               CurrentClientId = clientId,
               CurrentClientName = u.UserClients.Where(uc => uc.ClientId == clientId).FirstOrDefault().Client.Name,
               UniqueId = u.UniqueId,
               Email = u.Email,
               FirstName = u.FirstName,
               LastName = u.LastName,
               Id = u.Id,
               IsActive = u.IsActive,
               IsServicePrincipal = u.IsServicePrincipal.Value,
               IsServicePrincipalUser = u.IsServicePrincipal.Value,
               UserName = u.UserName,
               MiddleName = u.MiddleName,
               
               Modules = u.UserClientModules
                   .Where(ucm => ucm.Module != null && ucm.ClientId == clientId)
                   .Select(uc => uc.Module.Name).ToHashSet(),

               Permissions = u.UserRoles
                .Where(ur => ur.Role != null && ur.Role.ClientId == clientId)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Code)
                .ToHashSet(),

               DepartmentIds = u.UserDepartments
                   .Where(ud => ud.Department != null && ud.Department.ClientId == clientId)
                   .Select(ud => ud.Department.Id)
                   .ToHashSet(),

               DepartmentNames = u.UserDepartments
                   .Where(ud => ud.Department != null && ud.Department.ClientId == clientId)
                   .Select(ud => new DepartmentObj
                   {
                       Id = ud.DepartmentId,
                       DepartmentName = ud.Department.Name,

                   })
                   .ToList()
           })
           .SingleOrDefaultAsync();

            if (user == null) throw new UserNotFoundException();

            await _memoryCache.SetAsync<UserWithClients>($"{uniqueId}-{clientId}", user, new DistributedCacheEntryOptions());
             return await GetUserById(uniqueId,clientId);

        }

        public async Task RemoveCache(string key)
        {
            await _memoryCache.RemoveAsync(key);
        }
        private EmailTemplate? GetFluidTemplate(string templateId)
        {
            var notifyTemplate = _memoryCache.GetAsync<NotificationTemplate>(templateId).Result;

            if (notifyTemplate == null)
            {
                var template = _iamDbConext.NotificationTemplates.SingleOrDefault(s => s.Name == templateId);

                if (template == null)
                    return null;

                _memoryCache.SetAsync(templateId, template, new DistributedCacheEntryOptions()).Wait();
                notifyTemplate = template;
            }

            var parser = new FluidParser();
            if (parser.TryParse(notifyTemplate.Subject, out var subjectFluidTemplate, out var error) &&
                parser.TryParse(notifyTemplate.Body, out var bodyFluidTemplate, out var bodyerror))
            {
                var subjectLiquidTemplate = new FluidTemplateAdapter(subjectFluidTemplate);
                var bodyLiquidTemplate = new FluidTemplateAdapter(bodyFluidTemplate);

                var emailTemplate = new EmailTemplate
                {
                    Subject = subjectLiquidTemplate,
                    Body = bodyLiquidTemplate,
                    To = notifyTemplate.To?.Split(',').ToList() ?? new List<string>(),
                    Cc = notifyTemplate.Cc?.Split(',').ToList() ?? new List<string>(),
                    Bcc = notifyTemplate.Bcc?.Split(',').ToList() ?? new List<string>(),
                    Priority = notifyTemplate.Priority,
                    IsBodyHtml = notifyTemplate.IsBodyHtml
                };

                return emailTemplate;
            }

            return null;
        }

        public async Task<Result<List<GetAccessibleClientResponse>>> GetAccessibleClientAsync(string email)
        {
            var results = await _iamDbConext.UserClients
                .Where(t => t.User.Email == email && t.Client.Status.Name == ClientStatusTypes.Active)
                .ProjectTo<GetAccessibleClientResponse>(_mapper.ConfigurationProvider).ToListAsync();

            return Result<List<GetAccessibleClientResponse>>.Success(results);
        }
    }
}
