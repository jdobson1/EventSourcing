using EventStore;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Products.Query.Projections;
using Projections;
using System;
using Core.Infrastructure;

[assembly: FunctionsStartup(typeof(Products.Query.Startup))]

namespace Products.Query
{
    public class Startup : FunctionsStartup, IEventTypeResolver
    {
        private static readonly string EndpointUrl = Environment.GetEnvironmentVariable("CosmosEndpointUrl");
        private static readonly string AuthorizationKey = Environment.GetEnvironmentVariable("CosmosAuthorizationKey");
        private static readonly string MaterializedViewDatabaseId = Environment.GetEnvironmentVariable("CosmosMaterializedViewDatabaseId");

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
            builder.Services.AddTransient<ITenantViewRepository>(s =>
            {
                var cosmosClientFactory = s.GetRequiredService<ICosmosClientFactory>();
                return new TenantCosmosViewRepository(EndpointUrl, AuthorizationKey, MaterializedViewDatabaseId, cosmosClientFactory);
            });
            
            builder.Services.AddSingleton(s =>
            {
                var viewRepository = s.GetRequiredService<ITenantViewRepository>();
                return InitializeProjectionEngine(viewRepository);
            });
        }

        public Type GetEventType(string typeName)
        {
            return Type.GetType($"Products.Common.Events.{typeName}, Products.Common");
        }

        private ITenantProjectionEngine InitializeProjectionEngine(ITenantViewRepository viewRepository)
        {
            var projectionEngine = new TenantProjectionEngine(this, viewRepository);

            projectionEngine.RegisterProjection(new ProductsProjection());

            return projectionEngine;
        }
    }
}