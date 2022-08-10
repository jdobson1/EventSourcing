using EventStore;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Products.Query.Projections;
using Projections;
using System;

[assembly: FunctionsStartup(typeof(Products.Query.Startup))]

namespace Products.Query
{
    public class Startup : FunctionsStartup, IEventTypeResolver
    {
        private static string EndpointUrl = Environment.GetEnvironmentVariable("CosmosEndpointUrl"); //"https://cosmos-es.documents.azure.com:443/";
        private static readonly string AuthorizationKey = Environment.GetEnvironmentVariable("CosmosAuthorizationKey"); //"Pq3WO6OdB6yaryG6AdQgGleV0q210Eu60coHoyvJMhTH0abvE5EgEmywagoGv41rOLbcnpRSQHtRHowmwPQdhA==";
        private static string DatabaseId = Environment.GetEnvironmentVariable("CosmosEventStoreDatabaseId"); //"event-store";
        
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IViewRepository>(s => 
            {
                return new CosmosViewRepository(EndpointUrl, AuthorizationKey, DatabaseId);
            });

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