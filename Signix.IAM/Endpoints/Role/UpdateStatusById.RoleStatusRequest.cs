using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Signix.IAM.API.Endpoints.Role
{
    public class RoleStatusRequest
    {
        [FromRoute]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [FromBody]

        public bool IsActive { get; set; }
    }
}