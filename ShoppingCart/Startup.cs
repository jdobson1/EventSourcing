using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using EventStore;
using Core.Domain;
using Core.Infrastructure;
using Microsoft.Azure.Cosmos;
using ShoppingCart.Infrastructure.Repositories;
using Sagas;
using Sagas.StateProviders;
using ShoppingCart.Infrastructure.Sagas;

[assembly: FunctionsStartup(typeof(ShoppingCart.Startup))]

namespace ShoppingCart
{
    public class Startup : FunctionsStartup, IEventTypeResolver
    {
        private static readonly string EndpointUrl = Environment.GetEnvironmentVariable("CosmosEndpointUrl");
        private static readonly string AuthorizationKey = Environment.GetEnvironmentVariable("CosmosAuthorizationKey");
        private static readonly string DatabaseId = Environment.GetEnvironmentVariable("CosmosEventStoreDatabaseId");
        private static readonly string SagaDatabaseId = Environment.GetEnvironmentVariable("CosmosSagaDatabaseId");
        private static readonly string SagaStateContainerId = Environment.GetEnvironmentVariable("CosmosSagaStateContainerId");

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();
            builder.Services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
            builder.Services.AddSingleton<ICosmosDatabaseUserManager, CosmosDatabaseUserManager>();
            builder.Services.AddSingleton<CosmosDatabaseContext>(x => new CosmosDatabaseContext(EndpointUrl,
                AuthorizationKey, SagaStateContainerId, SagaDatabaseId, 18000,
                new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway }));
            builder.Services.AddTransient<IRepository<Domain.ShoppingCart>, ShoppingCartRepository>();
            builder.Services.AddSingleton<IEventStore>((s) =>
            {
                var cosmosUserManager = s.GetRequiredService<ICosmosDatabaseUserManager>();
                return new CosmosEventStore(this, EndpointUrl, AuthorizationKey, DatabaseId,
                        new CosmosClientFactory(cosmosUserManager), "shoppingcart");
            });

            builder.Services.AddSingleton(s =>
            {
                var cosmosClientFactory = s.GetRequiredService<ICosmosClientFactory>();
                return InitializeSagaEngine(cosmosClientFactory);
            });
        }

        public Type GetEventType(string typeName)
        {
            if (typeName.Contains("OrderSubmitted")) return Type.GetType($"Orders.Common.Events.{typeName}, Orders.Common");

            return Type.GetType($"ShoppingCart.Common.Events.{typeName}, ShoppingCart.Common");
        }

        private ISagaEngine InitializeSagaEngine(ICosmosClientFactory cosmosClientFactory)
        {
            var sagaEngine = new SagaEngine(this);
            sagaEngine.RegisterSaga(new ShoppingCartSaga(new CosmosStateProvider(cosmosClientFactory,
                new CosmosDatabaseContext(EndpointUrl, AuthorizationKey, SagaStateContainerId, SagaDatabaseId, 18000,
                    new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway }))));

            return sagaEngine;
        }
    }
}