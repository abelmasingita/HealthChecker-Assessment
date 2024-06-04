using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Types;
using HealthChecker.Repository;
using static HealthChecker.GraphQL.ServerType;

namespace HealthChecker.GraphQL
{
    public class Server
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string HealthCheckUri { get; set; }
        public string Status { get; set; }
        public DateTime? LastTimeUp { get; set; }
        public ErrorDetail Error { get; set; }
        public bool Disabled { get; set; } = false;
    }

    public class ErrorDetail
    {
        public int Status { get; set; }
        public string Body { get; set; }
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

            Field<ErrorDetailType>(
                "error",
                resolve: context =>
                {
                    var server = context.Source as Server;
                    return server.Error;
                }
            );
        }

        public class ErrorDetailType : ObjectGraphType<ErrorDetail>
        {
            public ErrorDetailType()
            {
                Field(e => e.Status);
                Field(e => e.Body);
            }
        }
    }


    public class HealthCheckerQuery : ObjectGraphType<object>
    {
        private List<Server> servers;
        public HealthCheckerQuery(List<Server> servers)
        {
            this.servers = servers;
            Name = "Query";

            Func<ResolveFieldContext, string, string, object> serverResolver = (context, id, status) =>
            {

                if (id != null && status != null)
                {
                    // Filter servers that match both the id and status
                    var filteredServers = this.servers.Where(s => s.Id == id && s.Status == status).ToList();
                    return filteredServers;
                }
                else if (id != null)
                {
                    // Filter servers by id
                    var filteredServersById = this.servers.Where(s => s.Id == id).ToList();
                    return filteredServersById;
                }
                else if (status != null)
                {
                    // Filter servers by status
                    var filteredServersByStatus = this.servers.Where(s => s.Status == status).ToList();
                    return filteredServersByStatus;
                }
                else
                {
                    return this.servers;
                }
            };

            FieldDelegate<ListGraphType<ServerType>>(
                "servers",
                arguments: new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "id", Description = "ID of the server to retrieve" },
                    new QueryArgument<StringGraphType> { Name = "status", Description = "Status of the server to filter by" }
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
        public HealthCheckerSchema(IServiceProvider provider, List<Server> servers) : base(provider)
        {
            Query = new HealthCheckerQuery(servers);
            Mutation = new HealthCheckerMutation(servers);
            RegisterType<ErrorDetailType>();
            RegisterType<ServerType>();
            RegisterType<DisableServerInputType>();
        }
    }

    public class DisableServerInputType : InputObjectGraphType
    {
        public DisableServerInputType()
        {
            Name = "DisableServerInput";
            Field<NonNullGraphType<StringGraphType>>("id", "ID of the server to disable");
        }
    }
    public class DisableServerInput
    {
        public string Id { get; set; }
    }
    public class HealthCheckerMutation : ObjectGraphType
    {
        private List<Server> servers;
        public HealthCheckerMutation(List<Server> servers)
        {
            this.servers = servers;

            Field<BooleanGraphType>(
                "disableServer",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<DisableServerInputType>> { Name = "input" }
                ),
                resolve: context =>
                {
                    var input = context.GetArgument<DisableServerInput>("input");
                    var server = servers.FirstOrDefault(s => s.Id == input.Id);
                    if (server == null)
                    {
                        context.Errors.Add(new ExecutionError("Server not found."));
                        return false;
                    }
                    server.Disabled = true;
                    return true;
                }
            );
        }
    }
}
