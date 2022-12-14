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
        private static readonly string DatabaseId = Environment.GetEnvironmentVariable("CosmosEventStoreDatabaseId");
        
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IViewRepository>(s => new CosmosViewRepository(EndpointUrl, AuthorizationKey, DatabaseId, s.GetRequiredService<ICosmosDatabaseUserManager>()));
            builder.Services.AddSingleton<ICosmosDatabaseUserManager, CosmosDatabaseUserManager>();
            builder.Services.AddSingleton(s => new CosmosClientFactory());
            builder.Services.AddSingleton(s =>
            {
                var viewRepository = s.GetRequiredService<IViewRepository>();
                return InitializeProjectionEngine(viewRepository);
            });
        }

        public Type GetEventType(string typeName)
        {
            return Type.GetType($"Products.Common.Events.{typeName}, Products.Common");
        }

        private IProjectionEngine InitializeProjectionEngine(IViewRepository viewRepository)
        {
            var projectionEngine = new ProjectionEngine(this, viewRepository);

            projectionEngine.RegisterProjection(new ProductsProjection());

            return projectionEngine;
        }
    }
}