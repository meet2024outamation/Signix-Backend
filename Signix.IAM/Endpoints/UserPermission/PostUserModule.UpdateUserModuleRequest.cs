namespace Signix.IAM.API.Endpoints.UserPermission;

public class UpdateUserModuleRequest
{
    public int UserId { get; set; }
    public int ModuleId { get; set; }
    public string ClientId { get; set; }
}
