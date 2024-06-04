using HealthChecker.GraphQL;
using HealthChecker.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecker.Services
{
    public class HealthCheckBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HealthCheckBackgroundService> _logger;
        private List<Server> servers;
        public HealthCheckBackgroundService(IServiceProvider serviceProvider, ILogger<HealthCheckBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            servers = new List<Server>{
                    new Server{
                        Id = "1",
                        Name = "stackworx.io",
                        HealthCheckUri = "https://www.stackworx.io",
                    },
                    new Server{
                        Id = "2",
                        Name = "prima.run",
                        HealthCheckUri = "https://prima.run",
                    },
                    new Server{
                        Id = "3",
                        Name = "google",
                        HealthCheckUri = "https://www.google.com",
                    } };
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var healthCheckService = scope.ServiceProvider.GetRequiredService<IHealthCheckService>();
                //var updatedServers = await healthCheckService.CheckAllHealthAsync(servers);

                _logger.LogInformation("Health check completed at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
