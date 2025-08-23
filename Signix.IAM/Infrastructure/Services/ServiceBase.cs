using SharedKernel.Services;
using Signix.IAM.Context;

namespace Signix.IAM.API.Infrastructure.Services
{
    public class ServiceBase : AuthService
    {
        protected readonly IAMDbContext _iamDbConext;

        public ServiceBase(IAMDbContext iamDbConext, IUser user) : base(user)
        {
            _iamDbConext = iamDbConext;
        }
    }
}
