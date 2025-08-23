using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Signix.IAM.API.Infrastructure.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Signix.IAM.API.Endpoints.Cache
{
    [Route("api/clearCache")]
    public class CacheCheck : EndpointBaseAsync
    .WithRequest<int>
    .WithActionResult<bool>
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IManageUserService _manageUserService;
        public CacheCheck(IDistributedCache distributedCache, IManageUserService manageUserService)
        {
            _distributedCache = distributedCache;
            _manageUserService = manageUserService;
        }
        [HttpGet("{check}")]
        [SwaggerOperation(Summary = "Clear Cache", Description = "1. Clear cache., 2. Check Cache data.", OperationId = "ClearCache.check", Tags = new[] { "Cache" }
      )]
        public override async Task<ActionResult<bool>> HandleAsync(int check, CancellationToken cancellationToken = default)
        {

            var allKeys = await _manageUserService.GetEmails();
            var cacheCount = allKeys.Count;
            if (check == 1)
            {
                foreach (string key in allKeys)
                {
                    await _distributedCache.RemoveAsync(key);
                }
                return Ok(cacheCount > 0);
            }
            if (check == 2)
            {
                cacheCount = 0;
                foreach (string key in allKeys)
                {
                    var cacheData = await _distributedCache.GetAsync(key);
                    if (cacheData != null)
                    {
                        cacheCount++;
                    }
                }
            }
            return Ok(cacheCount > 1);
        }
    }
}
