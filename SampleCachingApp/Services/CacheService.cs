using Microsoft.Extensions.Caching.Distributed;

namespace SampleCachingApp.Services
{
    public class CacheService
    {
        private readonly IDistributedCache _cache;

        private static string baseDirectory;
        private static string fullPath1;
        private static string fullPath2;
        private static string CacheVersion;


        public CacheService(IDistributedCache cache)
        {
            baseDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

            fullPath1 = Path.Combine(baseDirectory, @"Services\ExtensionUtility.cs");
            fullPath2 = Path.Combine(baseDirectory, @"Services\EmployeeService.cs");

            //Reference of 3 methods in 2 files, where the code change will be considered for cache invalidation.
            CacheVersion = CacheHelper.GetCombinedMethodCodeHash(new Dictionary<string, List<string>>
        {
            { fullPath1, new List<string> { "DynamicFilters", "DynamicSorting" } },
            { fullPath2, new List<string> { "GetEmployees" } }
        });

            _cache = cache;
        }

        public async Task SetCacheAsync(string key, string value)
        {
            string currentCacheKeyForParameters = await _cache.GetStringAsync(key);
            string comparisonVersionedKey = $"{CacheVersion}:{key}";

            //Fetching previous Hashed key and compare it with current one (removal if both arent same)
            if (!string.IsNullOrEmpty(currentCacheKeyForParameters) && currentCacheKeyForParameters != CacheVersion)
            {
                await _cache.RemoveAsync(key);
                await _cache.RemoveAsync(comparisonVersionedKey);
            }

            string versionedKey = $"{CacheVersion}:{key}";
            await _cache.SetStringAsync(versionedKey, value, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
            //setting hashed key as a value to fetch it later for removal when not in use.
            await _cache.SetStringAsync(key, CacheVersion, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
        }
        public async Task<string> GetCacheAsync(string key)
        {
            string versionedKey = $"{CacheVersion}:{key}";
            return await _cache.GetStringAsync(versionedKey);
        }

    }
}
