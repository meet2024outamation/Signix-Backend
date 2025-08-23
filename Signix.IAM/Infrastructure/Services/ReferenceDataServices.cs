//using Dapper;
//using SharedKernel.Result;
//using SharedKernel.Services;
//using Signix.IAM.API.Shared;
//using Signix.IAM.ViewModels;

//namespace Signix.IAM.API.Infrastructure.Services
//{
//    public class ReferenceDataServices : AuthService, IReferenceDataServices
//    {
//        private readonly DbConnection _db;

//        public ReferenceDataServices(DbConnection db, IUser user) : base(user)
//        {
//            _db = db;
//        }

//        public async Task<Result<IDictionary<string, IEnumerable<LookupsVM.LookupRes>>>> GetLookupsByNamesAsync(string[] names)
//        {
//            var lookupNames = LookupEnum.FromStrings(names);
//            if (lookupNames.Count() == 0)
//            {
//                lookupNames = LookupEnum.List();
//            }

//            var buildLookupQueries = new List<string>();
//            var results = new Dictionary<string, IEnumerable<LookupsVM.LookupRes>>();

//            foreach (var lookup in lookupNames)
//            {
//                if (Equals(lookup, LookupEnum.ClientTypes))
//                {
//                    buildLookupQueries.Add("SELECT Id, Name FROM [dbo].[ClientTypes] WITH(NOLOCK) WHERE IsActive = 1 ORDER BY Name;");
//                }
//                if (Equals(lookup, LookupEnum.ClientStatuses))
//                {
//                    buildLookupQueries.Add("SELECT Id, Name FROM [dbo].[ClientStatuses] WITH(NOLOCK) WHERE IsActive = 1 ORDER BY Name;");
//                }
//                if (Equals(lookup, LookupEnum.VendorTypes))
//                {
//                    buildLookupQueries.Add("SELECT Id, Name FROM [dbo].[VendorTypes] WITH(NOLOCK) WHERE IsActive = 1 ORDER BY Name;");
//                }
//                if (Equals(lookup, LookupEnum.Roles))
//                {
//                    buildLookupQueries.Add($"SELECT Id, Name FROM [dbo].[Roles] WITH(NOLOCK) WHERE ClientId = '{_user.CurrentClientId}' AND StatusId = (SELECT Id FROM [dbo].[RoleStatuses] WHERE  [Name] = 'Active')  ORDER  BY Name;");
//                }
//                if (Equals(lookup, LookupEnum.RoleStatuses))
//                {
//                    buildLookupQueries.Add($"SELECT Id, Name FROM [dbo].[RoleStatuses] WITH(NOLOCK) WHERE  IsActive = 1 ORDER  BY Name;");
//                }
//                if (Equals(lookup, LookupEnum.UserModules))
//                {
//                    buildLookupQueries.Add($"SELECT Id, Name FROM [dbo].[Modules] WITH(NOLOCK) ORDER  BY Name;");
//                }
//            }

//            var dbResult = await _db.QueryMultipleAsync(string.Join("", buildLookupQueries));

//            foreach (var lookup in lookupNames)
//            {
//                results.Add(lookup.Name, dbResult.Read<LookupsVM.LookupRes>().AsEnumerable());
//            }
//            return results;
//        }
//    }
//}