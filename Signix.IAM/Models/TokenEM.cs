namespace Signix.IAM.API.Models;

public class TokenEM
{
    public required int UserId { get; set; }

    public required string JWTToken { get; set; }
}
