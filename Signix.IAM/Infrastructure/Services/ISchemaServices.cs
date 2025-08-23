using SharedKernel.Result;
using Signix.IAM.Entities;
using Signix.IAM.API.Endpoints.Client;
using Signix.IAM.API.Models;
using Signix.IAM.API.Endpoints.Client;
using Signix.IAM.API.Endpoints.StandardSchema;
using Microsoft.AspNetCore.Mvc;

namespace Signix.IAM.API.Infrastructure.Services
{
    public interface ISchemaServices
    {
        Task<Result<GetSchema>> GetClientAsync(string departmentId);
        Task<Result<string>> GetTemplate(string departmentId);
    }
}
