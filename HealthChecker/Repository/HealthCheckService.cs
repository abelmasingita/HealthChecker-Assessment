using HealthChecker.GraphQL;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HealthChecker.Repository
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HealthCheckService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<(string Status, ErrorDetail Error, DateTime? LastTimeUp)> CheckHealthAsync(string url)
        {
            var client = _httpClientFactory.CreateClient();
            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return ("UP", null, DateTime.UtcNow);
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    return ("DOWN", new ErrorDetail { Status = (int)response.StatusCode, Body = errorBody.Trim() }, null);
                }
            }
            catch (Exception ex)
            {
                return ("DOWN", new ErrorDetail { Status = 500, Body = ex.Message }, null);
            }
        }
    }
}
