using SharedKernal.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Services
{
    public class HMACAuthService
    {
        protected readonly IHMACUser _hmacUser;
        public HMACAuthService(IHMACUser hmacUser)
        {
              this._hmacUser = hmacUser;
        }
    }

    public interface IHMACUser
    {
        public int Id { get; }       
        public string? CurrentTenantId { get; }
        public string? CurrentTenantCode { get; }
        public bool IsActive { get; }
        public string? ConnectionString { get; }        
    }
}
