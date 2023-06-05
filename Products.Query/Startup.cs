using EventStore;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Products.Query.Projections;
using Projections;
using System;
using System.Collections.Generic;
using Core.Domain;
using Core.Infrastructure;
using Microsoft.Azure.Cosmos;
using Sagas;

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
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
            builder.Services.AddSingleton<ICosmosDatabaseUserManager, CosmosDatabaseUserManager>();
            builder.Services.AddSingleton<CosmosDatabaseContext>(x => new CosmosDatabaseContext(EndpointUrl,
                AuthorizationKey, "views", MaterializedViewDatabaseId, 18000,
                new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway }));
            builder.Services.AddTransient<ITenantViewRepository>(s =>
            {
                var cosmosClientFactory = s.GetRequiredService<ICosmosClientFactory>();
                var cosmosUserManager = s.GetRequiredService<ICosmosDatabaseUserManager>();
                return new TenantCosmosViewRepository(EndpointUrl, AuthorizationKey, MaterializedViewDatabaseId, cosmosClientFactory, cosmosUserManager);
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