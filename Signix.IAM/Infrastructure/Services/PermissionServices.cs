using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Result;
using Signix.IAM.API.Endpoints.Permission;
using Signix.IAM.API.Endpoints.Role;
using Signix.IAM.Context;

namespace Signix.IAM.API.Infrastructure.Services
{
    public class PermissionServices : IPermissionServices
    {
        private readonly IAMDbContext _context;
        private readonly IMapper _mapper;
        public PermissionServices(IAMDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;

        }
        public async Task<Result<List<GetPermissionResponse>>> GetPermissionAsync()
        {
            var permissions = await _context.Permissions.ProjectTo<GetPermissionResponse>(_mapper.ConfigurationProvider).ToListAsync();
            return Result<List<GetPermissionResponse>>.Success(permissions);
        }

        public async Task<Result<GetPermissionResponse>> GetPermissionByIdAsync(int id)
        {
            var permission = await _context.Permissions.Where(p => p.Id == id).ProjectTo<GetPermissionResponse>(_mapper.ConfigurationProvider).SingleAsync();
            return Result<GetPermissionResponse>.Success(permission);
        }
    }
}
