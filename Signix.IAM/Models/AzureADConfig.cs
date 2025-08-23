namespace Signix.IAM.API.Models
{
    public class AzureADConfig
    {

        public string TenantId { get; set; } = string.Empty;
        public string CurrentClientId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string Instance { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string InviteRedirectUrl { get; set; } = string.Empty;
        public object ClientAppName { get; set; } = string.Empty;
        public string MasterClientId { get; set; } = string.Empty;
        public string MasterClientSecret { get; set; } = string.Empty;
    }

    public class SystemSetting
    {
        public string BaseClientCode { get; set; } = string.Empty;
    }
}
