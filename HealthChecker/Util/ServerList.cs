using HealthChecker.GraphQL;
using System.Collections.Generic;

namespace HealthChecker.Util
{
    public class ServerList
    {
        public List<Server> GetServers()
        {
            var servers = new List<Server>
            {
                new Server { Id = "1", Name = "stackworx.io", HealthCheckUri = "https://www.stackworx.io" },
                new Server { Id = "2", Name = "prima.run", HealthCheckUri = "https://prima.run" },
                new Server { Id = "3", Name = "google", HealthCheckUri = "https://www.google.com" }
            };

            return servers;
        }
    }
}
