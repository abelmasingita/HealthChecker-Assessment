using System.Threading.Tasks;
using System;

namespace HealthChecker.Repository
{
    public interface IHealthCheckService
    {
        Task<(string Status, string Error, DateTime? LastTimeUp)> CheckHealthAsync(string url);
    }
}
