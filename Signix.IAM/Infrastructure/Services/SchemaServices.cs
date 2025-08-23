using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Result;
using System.Data;
using System.Text.RegularExpressions;
using Signix.IAM.API.Endpoints.Client;
using Signix.IAM.Context;
using Signix.IAM.Entities;
using Dapper;
using System.Data.Common;
using AutoMapper.QueryableExtensions;
using static IAM.API.Infrastructure.Utility.Utility;
using Signix.IAM.API.Models;
using System.Security.Claims;
using SharedKernel.Services;
using Microsoft.Extensions.Options;
using Signix.IAM.API.Models;
using Signix.IAM.API.Endpoints.Client;
using Newtonsoft.Json;
using System.Text.Json;
using DocumentFormat.OpenXml.Bibliography;
using Signix.IAM.API.Endpoints.Department;
using Azure;
using Signix.IAM.API.Endpoints.StandardSchema;

namespace Signix.IAM.API.Infrastructure.Services
{
    public class SchemaServices : ServiceBase, ISchemaServices
    {
        private readonly IMapper _mapper;
        private readonly DbConnection _db;
        public SchemaServices(IAMDbContext iamDbConext,IMapper mapper,DbConnection db,IUser user) : base(iamDbConext, user)
        {
            _mapper = mapper;
            _db = db;
        }

        public async Task<Result<GetSchema>> GetClientAsync(string departmentId)
        {
            
            var client = _iamDbConext.Clients.FindAsync(_user.CurrentClientId).Result.StandardSchema;
            var department = _iamDbConext.Departments.FindAsync(departmentId).Result.StandardSchema;
            var schema = new GetSchema
            {
                ClientSchema = client.ToString(),
                DepartmentSchema = department.ToString()
            };
            return Result<GetSchema>.Success(schema);
        }

        public async Task<Result<string>> GetTemplate(string departmentId)
        {

            var departmentJson = _iamDbConext.Departments.FindAsync(departmentId).Result.SampleJson;

            return Result<string>.Success(departmentJson);
        }
    }
}
