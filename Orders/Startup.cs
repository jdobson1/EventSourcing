using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using EventStore;
using Core.Domain;
using Orders.Domain;
using Orders.Infrastructure.Repositories;
using Projections;
using Orders.Projections;

[assembly: FunctionsStartup(typeof(Orders.Startup))]

namespace Orders
{
    public class Startup : FunctionsStartup, IEventTypeResolver
    {
        private static string EndpointUrl = Environment.GetEnvironmentVariable("CosmosEndpointUrl");
        private static readonly string AuthorizationKey = Environment.GetEnvironmentVariable("CosmosAuthorizationKey");
        private static string DatabaseId = Environment.GetEnvironmentVariable("CosmosEventStoreDatabaseId");

        public override void Configure(IFunctionsHostBuilder builder)
        {
            //builder.Services.AddHttpClient();

            builder.Services.AddTransient<IRepository<Order>, OrderRepository>();
            builder.Services.AddSingleton<IEventStore>((s) =>
            {
                return new CosmosEventStore(this, EndpointUrl, AuthorizationKey, DatabaseId);
            });
            builder.Services.AddTransient<IViewRepository>(s =>
            {
                return new CosmosViewRepository(EndpointUrl, AuthorizationKey, DatabaseId);
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