using System;

namespace HealthCheckServer.Models
{
    public class ServerStatus
    {
        public int Id { get; set; }
        public string ServerId { get; set; }
        public string Status { get; set; }
        public DateTime LastTimeUp { get; set; }
    }

}
