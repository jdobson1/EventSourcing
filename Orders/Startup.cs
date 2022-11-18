using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using EventStore;
using Core.Domain;
using Core.Infrastructure;
using Orders.Domain;
using Orders.Infrastructure.Repositories;
using Projections;
using Orders.Projections;

[assembly: FunctionsStartup(typeof(Orders.Startup))]

namespace Orders
{
    public class Startup : FunctionsStartup, IEventTypeResolver
    {
        private static readonly string EndpointUrl = Environment.GetEnvironmentVariable("CosmosEndpointUrl");
        private static readonly string AuthorizationKey = Environment.GetEnvironmentVariable("CosmosAuthorizationKey");
        private static readonly string DatabaseId = Environment.GetEnvironmentVariable("CosmosEventStoreDatabaseId");

        public override void Configure(IFunctionsHostBuilder builder)
        {
            //builder.Services.AddHttpClient();

            builder.Services.AddTransient<IRepository<Order>, OrderRepository>();
            builder.Services.AddSingleton<ICosmosDatabaseUserManager, CosmosDatabaseUserManager>();
            builder.Services.AddSingleton<IEventStore>((s) => new CosmosEventStore(this, EndpointUrl, AuthorizationKey, DatabaseId, new CosmosClientFactory(), "orders"));
            builder.Services.AddTransient<IViewRepository>(s =>
            {
                var cosmosUserManager = s.GetRequiredService<ICosmosDatabaseUserManager>();
                return new CosmosViewRepository(EndpointUrl, AuthorizationKey, DatabaseId, cosmosUserManager);
            });
            builder.Services.AddSingleton(s =>
            {
                var viewRepository = s.GetRequiredService<IViewRepository>();
                return InitializeProjectionEngine(viewRepository);
            });
            // builder.Services.AddSingleton<ILoggerProvider, MyLoggerProvider>();
        }

        public Type GetEventType(string typeName)
        {
            var type = Type.GetType($"Orders.Common.Events.{typeName}, Orders.Common");

            return type ?? Type.GetType($"ShoppingCart.Common.Events.{typeName}, ShoppingCart.Common");

        }

        private IProjectionEngine InitializeProjectionEngine(IViewRepository viewRepository)
        {
            var projectionEngine = new ProjectionEngine(this, viewRepository);

            projectionEngine.RegisterProjection(new ShoppingCartProjection());

            return projectionEngine;
        }
    }
}