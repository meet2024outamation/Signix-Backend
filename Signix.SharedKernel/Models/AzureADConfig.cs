using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Models
{
    public class AzureADConfig
    {
        public string Instance { get; set; }
        public string Domain { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string InviteRedirectUrl { get; set; }
        public string ClientAppName { get; set; }
        public string MasterClientId { get; set; }
        public string MasterClientSecret { get; set; }
        public string GrantType { get; set; }
        public string Scopes { get; set; }
    }
}
