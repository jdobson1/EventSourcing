using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using EventStore;
using Products.Infrastructure.Repositories;
using Core.Domain;
using Core.Infrastructure;
using Products.Domain;

[assembly: FunctionsStartup(typeof(Products.Startup))]

namespace Products
{
    public class Startup : FunctionsStartup, IEventTypeResolver
    {
        private static readonly string EndpointUrl = Environment.GetEnvironmentVariable("CosmosEndpointUrl");
        private static readonly string AuthorizationKey = Environment.GetEnvironmentVariable("CosmosAuthorizationKey");
        private static readonly string DatabaseId = Environment.GetEnvironmentVariable("CosmosEventStoreDatabaseId");

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IRepository<Product>, ProductRepository>();
            builder.Services.AddSingleton<ICosmosClientFactory>(s => new CosmosClientFactory());
            builder.Services.AddSingleton<IEventStore>((s) => new CosmosEventStore(this, EndpointUrl, AuthorizationKey, DatabaseId, s.GetRequiredService<ICosmosClientFactory>(), "useradmin"));

            // builder.Services.AddSingleton<ILoggerProvider, MyLoggerProvider>();
        }

        public Type GetEventType(string typeName)
        {
            return Type.GetType($"Products.Common.Events.{typeName}, Products.Common");
        }
    }
}