using Microsoft.AspNetCore.Mvc;
using SampleCachingApp.Services;
using System.Text.Json;
using SampleCachingApp.Model;

namespace SampleCachingApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeService _employeeService;
        private readonly CacheService _cacheService;
        public EmployeeController(ILogger<EmployeeController> logger, EmployeeService employeeService, CacheService cacheService)
        {
            _employeeService = employeeService;
            _cacheService = cacheService;
        }
        //Main API Call (Converted get request in to post due to complex parameter structure)
        [HttpPost]
        [Route("FetchEmployees")]
        public async Task<IActionResult> FetchEmployees([FromBody] RequestObject obj)
        {
            try
            {
                string cacheKey = $"Cache_{JsonSerializer.Serialize(obj)}";

                var cachedResult = await _cacheService.GetCacheAsync(cacheKey);
                if (cachedResult != null)
                {
                    var deserializedResult = JsonSerializer.Deserialize<List<Employee>>(cachedResult);
                    return Ok(deserializedResult);
                }
                else
                {
                    var employees = _employeeService.GetEmployees(obj);

                    await _cacheService.SetCacheAsync(cacheKey, JsonSerializer.Serialize(employees));
                    return Ok(employees);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
