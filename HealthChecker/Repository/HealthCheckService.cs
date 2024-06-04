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

        public async Task<(string Status, string Error, DateTime? LastTimeUp)> CheckHealthAsync(string url)
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
                    return ("DOWN", $"Status Code: {response.StatusCode}, Body: {errorBody}", null);
                }
            }
            catch (Exception ex)
            {
                return ("DOWN", ex.Message, null);
            }
        }
    }
}
