using Microsoft.AspNetCore.Mvc;

namespace Signix.IAM.API.Endpoints.Client
{
    public class UpdateStatusByIdR
    {
        [FromRoute]
        public string Id { get; set; }

        [FromBody]

        public bool IsActive { get; set; }
    }
}
