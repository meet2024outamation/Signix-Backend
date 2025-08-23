using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Result;
using System.Data.Common;
using Signix.IAM.Context;
using Signix.IAM.Entities;

namespace Signix.IAM.API.Infrastructure.Services
{
    public class UserClientServices : IUserClientServices
    {
        private readonly IAMDbContext _context;
        public UserClientServices(IAMDbContext context)
        {
            _context = context;
        }
        public async Task UpdateUserClientAsync(string clientId, int userId)
        {
            var userClients = await _context.UserClients.Where(t => t.UserId == userId).FirstOrDefaultAsync();

            if (userClients!=null)
            {
                // since one user can only be part of one client we will remove the user from the existing client
                _context.UserClients.Remove(userClients);
                await _context.SaveChangesAsync();
            }

            await _context.UserClients.AddAsync(new UserClient { ClientId = clientId, UserId = userId });
            await _context.SaveChangesAsync();
        }
    }
}
