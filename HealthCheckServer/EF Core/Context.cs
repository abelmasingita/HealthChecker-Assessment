using HealthCheckServer.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthCheckServer.EF_Core
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {

        }

        public DbSet<ServerStatus> ServerStatuses { get; set; }
    }
}
