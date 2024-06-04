using System;
using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using HealthChecker.Repository;

namespace HealthChecker.GraphQL
{

    public class Server
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string HealthCheckUri { get; set; }
        public string Status { get; set; }
        public DateTime? LastTimeUp { get; set; }
        public string Error { get; set; }
    }

    public class ServerType : ObjectGraphType<Server>
    {
        public ServerType(IHealthCheckService healthCheckService)
        {
            Name = "Server";
            Description = "A server to monitor";

            Field(h => h.Id);
            Field(h => h.Name);
            Field(h => h.HealthCheckUri);

            FieldAsync<StringGraphType>(
                "status",
                resolve: async context =>
                {
                    var server = context.Source as Server;
                    var (status, error, lastTimeUp) = await healthCheckService.CheckHealthAsync(server.HealthCheckUri);
                    server.Status = status;
                    server.Error = error;
                    server.LastTimeUp = lastTimeUp;
                    return status;
                }
            );

            Field<DateTimeGraphType>(
                "lastTimeUp",
                resolve: context =>
                {
                    var server = context.Source as Server;
                    return server.LastTimeUp;
                }
            );

            Field<StringGraphType>(
                "error",
                resolve: context =>
                {
                    var server = context.Source as Server;
                    return server.Error;
                }
            );
        }
    }


    public class HealthCheckerQuery : ObjectGraphType<object>
    {
        private List<Server> servers = new List<Server>{
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
            },
        };

        public HealthCheckerQuery()
        {
            Name = "Query";


            Func<ResolveFieldContext, string, object> serverResolver = (context, id) => this.servers;

            FieldDelegate<ListGraphType<ServerType>>(
                "servers",
                arguments: new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "id", Description = "id of server" }
                ),
                resolve: serverResolver
            );

            Field<StringGraphType>(
                "hello",
                resolve: context => "world"
            );
        }
    }

    public class HealthCheckerSchema : Schema
    {
        public HealthCheckerSchema(IServiceProvider provider) : base(provider)
        {
            Query = new HealthCheckerQuery();
        }
    }
}
