using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using EventStore;
using Products.Infrastructure.Repositories;
using Core.Domain;
using Products.Domain;

[assembly: FunctionsStartup(typeof(Products.Startup))]

namespace Products
{
    public class Startup : FunctionsStartup, IEventTypeResolver
    {
        private static string EndpointUrl = Environment.GetEnvironmentVariable("CosmosEndpointUrl");
        private static readonly string AuthorizationKey = Environment.GetEnvironmentVariable("CosmosAuthorizationKey");
        private static string DatabaseId = Environment.GetEnvironmentVariable("CosmosEventStoreDatabaseId");

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IRepository<Product>, ProductRepository>();
            builder.Services.AddSingleton<IEventStore>((s) =>
            {
                return new CosmosEventStore(this, EndpointUrl, AuthorizationKey, DatabaseId);
            });

            // builder.Services.AddSingleton<ILoggerProvider, MyLoggerProvider>();
        }

        public Type GetEventType(string typeName)
        {
            return Type.GetType($"Products.Common.Events.{typeName}, Products.Common");
        }
    }
}