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
using Signix.IAM.API.Properties;
using System.Text;

namespace Signix.IAM.API.Infrastructure.Services
{
    public class ClientServices : ServiceBase, IClientServices
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly DbConnection _db;
        private readonly IManageUserService _userService;
        private readonly IMemCacheService _memCacheService;
        private readonly AzureADConfig _azureADConfig;
        private readonly IEmailServices _emailServices;
        private readonly IManageUserService _manageUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;

        public ClientServices(IManageUserService manageUserService ,IAMDbContext iamDbConext, IMapper mapper, IConfiguration configuration, DbConnection db, IManageUserService userService, IMemCacheService memCacheService, IOptions<AzureADConfig> azureADConfig, IEmailServices emailServices, IUser user, IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory) : base(iamDbConext, user)
        {
            _manageUserService = manageUserService;
            _mapper = mapper;
            _configuration = configuration;
            _db = db;
            _userService = userService;
            _memCacheService = memCacheService;
            _azureADConfig = azureADConfig.Value;
            _emailServices = emailServices;
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<Result<string>> CreateClientAsync(ClientCreateRequest ClientCR)
        {
            if (await IsClientNameExists(ClientCR.ClientName))
            {
                return Result<string>.Invalid(new List<ValidationError> { new ValidationError { Key = "Client", ErrorMessage = "Client name already exists" } });
            }
            var client = _httpClientFactory.CreateClient(nameof(UrlsConfig.DocsUrl));

            var ClientStatuses = await _iamDbConext.ClientStatuses.ToListAsync();
            Client Client = _mapper.Map<Client>(ClientCR);
            Client.CreatedById = Client.ModifiedById = _user.Id;
            Client.StatusId = ClientStatuses.Single(t => t.Name == ClientStatusTypes.Active).Id;
            Client.Id = Guid.NewGuid().ToString();
            Client.Identifier = Guid.NewGuid().ToString();
            Client.Code = $"ODocs{Regex.Replace(Client.Name, "[^a-zA-Z0-9]", "")}".ToLower();
            var tenantId = Client.Id;
            byte[] schemaBytes = Resource.Client_StandardSchema;
            string jsonSchema = Encoding.UTF8.GetString(schemaBytes);
            Client.StandardSchema = jsonSchema;
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/tenant-configuration?tenantId={tenantId}")
            {
                Content = new StringContent("", System.Text.Encoding.UTF8, "application/json") 
            };

            var response1 = await client.SendAsync(request);
            response1.EnsureSuccessStatusCode();
            var id = await response1.Content.ReadFromJsonAsync<int>();
          
            if (await IsDatabaseExists(Client.Code))
            {
                for (int i = 1; i < 10; i++)
                {
                    Client.Code = $"{Client.Code}{i}";
                    if (!await IsDatabaseExists(Client.Code))
                    {
                        break;
                    }
                }
            }

            using var tran = await _iamDbConext.Database.BeginTransactionAsync();
            try
            {
                await _iamDbConext.Clients.AddAsync(Client);
                var existsUser = await _userService.GetUserByEmail(ClientCR.EmailAddress);
                if (existsUser.IsSuccess && existsUser.Value != null)
                {
                    if (!existsUser.Value.IsActive)
                        return Result<string>.Invalid(new List<ValidationError> { new ValidationError { Key = "Client", ErrorMessage = $"Client admin cannot be inactive." } });

                    Client.UserClients.Add(new UserClient { UserId = existsUser.Value.Id!.Value });
                    if (existsUser.Value.CurrentClientId.IsNullOrWhiteSpace())
                    {
                        existsUser.Value.CurrentClientId = Client.Id;
                        _iamDbConext.Update(existsUser.Value);
                    }
                    await _iamDbConext.SaveChangesAsync();
                    //Send Email 
                    var ClientAssignment = _memCacheService.ClientAssignment;
                    if (ClientAssignment == null)
                    {
                        return Result<string>.Invalid(new List<ValidationError> { new ValidationError { Key = "Client", ErrorMessage = $"Email template is not found." } });
                    }
                    var ClientObj = new
                    {
                        DisplayName = $"{_user.FirstName} {_user.LastName}",
                        InviteLink = _azureADConfig.InviteRedirectUrl,
                        AppName = _azureADConfig.ClientAppName,
                        ClientName = ClientCR.ClientName
                    };
                    ClientAssignment.To.Add(_user.Email);
                    await _emailServices.SendEmailWithTemplateAsync(ClientAssignment, ClientObj);
                }
                //else
                //{
                //    var user = await _userService.CreateUser(new UserCM
                //    {
                //        Email = ClientCR.InitialAdminEmail,
                //        FirstName = ClientCR.InitialAdminFirstName,
                //        //MiddleName = ClientCR.InitialAdminMiddleName,
                //        LastName = ClientCR.InitialAdminLastname,
                //        IsActive = true,
                //        ClientIds = Client.Id 
                //    }, true);
                //}
                int result = await _iamDbConext.SaveChangesAsync();
                //if (Client.DatabaseCreationSource.Equals("Auto"))
                //{
                //    var IsDatabaseCreated = await CreateDatabaseAsync(Client.Id);
                //    if (IsDatabaseCreated.Value)
                //    {
                //        var IsDataSeeded = await SeedDataAsync(Client.Id);
                //        if (IsDataSeeded.Value && ClientCR.IsActive)
                //        {
                //            Client = (await _iamDbConext.Clients.FindAsync(Client.Id))!;
                //            Client.StatusId = ClientStatuses.Single(t => t.Name == ClientStatusTypes.Active).Id;
                //            _iamDbConext.Update(Client);
                //            result = await _iamDbConext.SaveChangesAsync();
                //        }
                //    }
                //}
                await tran.CommitAsync();
                var response = new
                {
                    Id = Client.Id
                };
                return Result<string>.Success(System.Text.Json.JsonSerializer.Serialize(response));
            }

            catch (Exception e)
            {
                tran.Rollback();
                var validationErrors = new List<ValidationError>
                {
       new ValidationError { Key = "Exception", ErrorMessage = e.Message }
                    };
                return Result<string>.Invalid(validationErrors);

            }
        }
        public async Task<Result<bool>> CreateDatabaseAsync(string ClientId)
        {
            Client? Client = await _iamDbConext.Clients.FindAsync(ClientId);
            if (Client == null)
            {
                return Result<bool>.Invalid(new List<ValidationError> { new ValidationError { Key = "Client", ErrorMessage = "Client not found" } });
            }
            await _db.ExecuteAsync(@$"create database ""{Client.Code}""");
            //Client.DatabaseCreatedDateTime = DateTimeOffset.UtcNow;
            Client.ConnectionString = _configuration.GetConnectionString("Client")?.Replace("{databasename}", Client.Code);
            //Client.IsDatabaseCreated = true;
            _iamDbConext.Clients.Update(Client);
            await _iamDbConext.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
        public async Task<Result<bool>> SeedDataAsync(string ClientId)
        {
            Client? Client = await _iamDbConext.Clients.FindAsync(ClientId);
            if (Client == null)
            {
                return Result<bool>.Invalid(new List<ValidationError> { new ValidationError { Key = "Client", ErrorMessage = "Client not found" } });
            }
            //string seedScript = Properties.Resources.seedscript.Replace("{databasename}", Client.Code);
            //using SqlConnection sqlConnection = new(Client.ConnectionString);
            //Server server = new(new ServerConnection(sqlConnection));
            //server.ConnectionContext.ExecuteNonQuery(seedScript);
            //Client.IsDataSeeded = true;
            _iamDbConext.Update(Client);
            await _iamDbConext.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
        private async Task<bool> IsDatabaseExists(string databaseName)
        {
            return await _db.QuerySingleAsync<bool>(@$"if exists (SELECT [name] FROM sys.databases where [Name] = @databaseName) begin select 1 end else begin select 0 end", new { databaseName });
        }
        private async Task<bool> IsReferralCodeExists(string referralCode)
        {
            return await _db.QuerySingleAsync<bool>(@"if exists (select 1 from Clients where ReferralCode = @referralCode) begin select 1 end else begin select 0 end", new { referralCode });
        }
        private async Task<bool> IsClientNameExists(string ClientName)
        {
            return await _db.QuerySingleAsync<bool>(@"if exists (select 1 from Clients where Name = @ClientName) begin select 1 end else begin select 0 end", new { ClientName });
        }

        public async Task<Result<ClientGetResponse>> GetClientByIdAsync(string id)
        {
            var activeUsers = await _manageUserService.GetUsers(true, _user.CurrentClientId);
            Client? Client = await _iamDbConext.Clients
                .Where(t => t.Id == id).SingleAsync();
            if (Client == null)
            {
                return Result<ClientGetResponse>.Invalid(new List<ValidationError> { new ValidationError { Key = "Client", ErrorMessage = "Client not found" } });
            }
            var ct = _mapper.Map<ClientGetResponse>(Client);
            ct.CreatedBy = activeUsers.SingleOrDefault(u => u.Id == Client.CreatedById)?.Name ?? "-";
            
            return Result<ClientGetResponse>.Success(ct);
        }

        public async Task<Result<string>> UpdateClientAsync(ClientEditRequest ClientER)
        {
            Client? Client = await _iamDbConext.Clients.FindAsync(ClientER.Id);
            if (Client == null)
            {
                return Result<string>.Invalid(new List<ValidationError> { new ValidationError { Key = "Client", ErrorMessage = "Client not found" } });
            }
            Client = _mapper.Map<ClientEditRequest, Client>(ClientER, Client);
            Client.ModifiedById = _user.Id;
            //if (ClientER.StatusName.Equals("Active"))
            //{
            //    if (Client.IsDatabaseCreated == null || !Client.IsDatabaseCreated.Value)
            //    {
            //        return Result<int>.Invalid(new List<ValidationError> { new ValidationError { Key = "Client", ErrorMessage = "Client can't be active. Database not yet created" } });
            //    }
            //    if (!Client.IsDataSeeded)
            //    {
            //        return Result<int>.Invalid(new List<ValidationError> { new ValidationError { Key = "Client", ErrorMessage = "Client can't be active. Default data not yet seeded" } });
            //    }
            //}
            var clientStatuses = await _iamDbConext.ClientStatuses.ToListAsync();
            Client.StatusId = ClientER.IsActive ? clientStatuses.FirstOrDefault(cs => cs.Name == "Active")?.Id : clientStatuses.FirstOrDefault(cs => cs.Name == "Inactive")?.Id;
            _iamDbConext.Update<Client>(Client);
            await _iamDbConext.SaveChangesAsync();
            var result = new
            {
                Id = Client.Id,

            };
            return Result<string>.Success(System.Text.Json.JsonSerializer.Serialize(result));
        }

        public async Task<Result<List<ClientGetResponse>>> GetClientAsync()
        {
            var activeUsers = await _manageUserService.GetUsers(true, _user.CurrentClientId);
            if (activeUsers == null)
            {
                return Result<List<ClientGetResponse>>.Invalid(new List<ValidationError> { new ValidationError { Key = "Expression", ErrorMessage = "Users not found" } });
            }
            var results = await _iamDbConext.Clients.OrderByDescending(c => c.CreatedDateTime).ToListAsync();
            List<ClientGetResponse> responses = new List<ClientGetResponse>();
            foreach (var client in results) {
               var ct = _mapper.Map<ClientGetResponse>(client);
                ct.CreatedBy = activeUsers.SingleOrDefault(u => u.Id == client.CreatedById)?.Name ?? "-";
                responses.Add(ct);

            }
            return Result<List<ClientGetResponse>>.Success(responses);
        }

        public async Task<Result<int>> ChangeCurrentClientAsync(ChangeClientRequest request, string email)
        {
            var user = await _iamDbConext.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return Result<int>.Invalid(new List<ValidationError> { new ValidationError { Key = "User", ErrorMessage = "User not found" } });
            }
            var userClients = await _iamDbConext.UserClients.AnyAsync(c => c.ClientId == request.ClientId && c.UserId == user.Id);
            if (!userClients)
            {
                return Result<int>.Forbidden();
            }
            user.CurrentClientId = request.ClientId;
            user.ModifiedById = user.Id;
            _iamDbConext.Update(user);

            var clientId = _httpContextAccessor.HttpContext!.Request.Headers["Client-Id"].ToString();
            int result = await _iamDbConext.SaveChangesAsync();
            await _memCacheService.RemoveCache(email);
            await _memCacheService.GetUserById(email, clientId);
            return Result<int>.Success(result);
        }

        public async Task<Result<List<GetAccessibleClientResponse>>> GetAccessibleClientAsync(string email)
        {
            var results = await _iamDbConext.UserClients
                .Where(t => t.User.Email == email && t.Client.Status.Name == ClientStatusTypes.Active)
                .ProjectTo<GetAccessibleClientResponse>(_mapper.ConfigurationProvider).ToListAsync();

            return Result<List<GetAccessibleClientResponse>>.Success(results);
        }

        public async Task<Result<string>> UpdateStatusByIdAsync(UpdateStatusByIdR request)
        {
            var client = await _iamDbConext.Clients.Where(ct => ct.Id == request.Id).FirstOrDefaultAsync();

            if (client == null)
            {
                return  Result<string>.Invalid(new List<ValidationError> { new ValidationError { Key = "Client", ErrorMessage = "Client not found" } });
            }

            client.IsActive = request.IsActive;
            client.ModifiedById = _user.Id;
            _iamDbConext.Update(client);
            await _iamDbConext.SaveChangesAsync();
            var result = new
            {
                Id = request.Id,
            };
            return Result<string>.Success(System.Text.Json.JsonSerializer.Serialize(result));
        }
    }
}
