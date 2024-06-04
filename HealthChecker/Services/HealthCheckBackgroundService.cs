using HealthChecker.GraphQL;
using HealthChecker.Repository;
using HealthChecker.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecker.Services
{
    public class HealthCheckBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HealthCheckBackgroundService> _logger;
        private List<Server> servers;
        ServerList serverList = new ServerList();
        public HealthCheckBackgroundService(IServiceProvider serviceProvider, ILogger<HealthCheckBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            this.servers = this.serverList.GetServers();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var enabledServers = servers.Where(s => !s.Disabled).ToList();

                    var healthCheckService = scope.ServiceProvider.GetRequiredService<IHealthCheckService>();
                    var updatedServers = await healthCheckService.CheckAllHealthAsync(enabledServers);

                    _logger.LogInformation("Health check completed at: {time}", DateTimeOffset.Now);
                }
                //await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);//for testing
                await Task.Delay(5, stoppingToken); // runs every 5 minutes
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HealthCheckBackgroundService is stopping.");
            await base.StopAsync(stoppingToken);
            _logger.LogInformation("HealthCheckBackgroundService has stopped.");
        }
    }
}
