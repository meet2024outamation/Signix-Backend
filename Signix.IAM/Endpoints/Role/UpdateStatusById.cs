using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Signix.IAM.API.Extensions;
using Signix.IAM.API.Infrastructure.Services;
using Signix.IAM.API.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Signix.IAM.API.Endpoints.Role
{
    public class UpdateStatusById : EndpointBaseAsync.WithRequest<RoleStatusRequest>.WithActionResult<ErrorMessageVM>
    {
        private readonly IRoleServices _roleServices;
        public UpdateStatusById(IRoleServices roleServices)
        {
            _roleServices = roleServices;
        }
        [HttpPatch("api/role/status/{id}")]
        [SwaggerOperation(Summary = "Update role status", Description = "", OperationId = "Role.UpdateStatusById", Tags = new[] { "Role" })]
        public override async Task<ActionResult<ErrorMessageVM>> HandleAsync(RoleStatusRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _roleServices.UpdateStatusByIdAsync(request);
            return result.ToActionResult(this);
        }
    }
}
