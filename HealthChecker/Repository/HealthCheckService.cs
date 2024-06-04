using HealthChecker.GraphQL;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HealthChecker.Repository
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HealthCheckService> _logger;
        public HealthCheckService(IHttpClientFactory httpClientFactory, ILogger<HealthCheckService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<List<Server>> CheckAllHealthAsync(List<Server> servers)
        {
            var tasks = servers.Select(async server =>
            {
                try
                {
                    var (status, error, lastTimeUp) = await CheckHealthAsync(server.HealthCheckUri).ConfigureAwait(false);
                    server.Status = status;
                    server.Error = error;
                    server.LastTimeUp = lastTimeUp;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking health for server {Server}", server.Name);
                    server.Status = "DOWN";
                    server.Error = new ErrorDetail { Status = 500, Body = ex.Message };
                    server.LastTimeUp = null;
                }

                return server;
            }).ToArray();

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            return results.ToList();
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
