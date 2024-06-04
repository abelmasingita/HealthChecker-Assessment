using System.Threading.Tasks;
using System;
using HealthChecker.GraphQL;
using System.Collections.Generic;

namespace HealthChecker.Repository
{
    public interface IHealthCheckService
    {
        Task<(string Status, ErrorDetail Error, DateTime? LastTimeUp)> CheckHealthAsync(string url);
        Task<List<Server>> CheckAllHealthAsync(List<Server> servers);

    }
}
