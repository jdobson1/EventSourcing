using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using EventStore;
using Core.Domain;
using Core.Infrastructure;
using ShoppingCart.Infrastructure.Repositories;

[assembly: FunctionsStartup(typeof(ShoppingCart.Startup))]

namespace ShoppingCart
{
    public class Startup : FunctionsStartup, IEventTypeResolver
    {
        private static readonly string EndpointUrl = Environment.GetEnvironmentVariable("CosmosEndpointUrl");
        private static readonly string AuthorizationKey = Environment.GetEnvironmentVariable("CosmosAuthorizationKey");
        private static readonly string DatabaseId = Environment.GetEnvironmentVariable("CosmosEventStoreDatabaseId");

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IRepository<Domain.ShoppingCart>, ShoppingCartRepository>();
            builder.Services.AddSingleton<IEventStore>((s) => new CosmosEventStore(this, EndpointUrl, AuthorizationKey, DatabaseId, new CosmosClientFactory()));

            // builder.Services.AddSingleton<ILoggerProvider, MyLoggerProvider>();
        }

        public Type GetEventType(string typeName)
        {
            return Type.GetType($"ShoppingCart.Common.Events.{typeName}, ShoppingCart.Common");
        }
    }
}