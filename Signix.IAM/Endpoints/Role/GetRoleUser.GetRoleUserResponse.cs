namespace Signix.IAM.API.Endpoints.Role
{
    public class GetRoleUserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
    }
}
