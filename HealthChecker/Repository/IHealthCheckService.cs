using System.Threading.Tasks;
using System;
using HealthChecker.GraphQL;

namespace HealthChecker.Repository
{
    public interface IHealthCheckService
    {
        Task<(string Status, ErrorDetail Error, DateTime? LastTimeUp)> CheckHealthAsync(string url);
    }
}
