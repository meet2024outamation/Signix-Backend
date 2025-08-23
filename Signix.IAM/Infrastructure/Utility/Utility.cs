using System.IdentityModel.Tokens.Jwt;

namespace Signix.IAM.API.Infrastructure.Utility
{
    public class Utility
    {
        public class ClientStatusTypes
        {
            public static string Inprogress = "Inprogress";
            public static string Cancelled = "Cancelled";
            public static string Active = "Active";
            public static string Inactive = "Inactive";
        }

        public class TokenDetails
        {
            public string Signature { get; set; } = null!;
            public DateTimeOffset TokenExpiryUtcDate { get; set; }
        }

        public static TokenDetails ParsedToken(string jwtTokenString)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(jwtTokenString);
            var expirationClaim = jwtToken.Claims.First(c => c.Type == "exp");
            long expirationUnixTime = long.Parse(expirationClaim.Value);

            return new TokenDetails { Signature = jwtToken.RawSignature, TokenExpiryUtcDate = DateTimeOffset.FromUnixTimeSeconds(expirationUnixTime) };
        }
    }
}
