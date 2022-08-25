using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using EventStore;
using Core.Domain;
using Projections;
using Subscriptions;
using Inventory.Infrastructure.Repositories;
using Inventory.Domain;
using Orders.Common.Events;
using Inventory.EventHandlers;
using Products.Common.Events;

[assembly: FunctionsStartup(typeof(Inventory.Startup))]

namespace Inventory
{
    public class Startup : FunctionsStartup, IEventTypeResolver
    {
        private static readonly string EndpointUrl = Environment.GetEnvironmentVariable("CosmosEndpointUrl");
        private static readonly string AuthorizationKey = Environment.GetEnvironmentVariable("CosmosAuthorizationKey");
        private static readonly string DatabaseId = Environment.GetEnvironmentVariable("CosmosEventStoreDatabaseId");

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IRepository<ProductInventory>, ProductInventoryRepository>();
            builder.Services.AddSingleton<IEventStore>((s) => new CosmosEventStore(this, EndpointUrl, AuthorizationKey, DatabaseId));
            builder.Services.AddSingleton((s) => InitializeSubscriptions());
           
            // builder.Services.AddSingleton<ILoggerProvider, MyLoggerProvider>();
        }

        public Type GetEventType(string typeName)
        {
            var type = Type.GetType($"Inventory.Common.Events.{typeName}, Inventory.Common");

            if (type != null) return type;

            type = Type.GetType($"Orders.Common.Events.{typeName}, Orders.Common");

            if (type != null) return type;

            type = Type.GetType($"Products.Common.Events.{typeName}, Products.Common");

            if (type != null) return type;

            return Type.GetType($"ShoppingCart.Common.Events.{typeName}, ShoppingCart.Common");
        }

        private ISubscriptionEngine InitializeSubscriptions()
        {
            ISubscriptionEngine subscriptionEngine = new SubscriptionEngine(this, EndpointUrl, "inventory", AuthorizationKey, DatabaseId);
            subscriptionEngine.Subscribe(
                new Subscription
                {
                    EventType = typeof(OrderSubmitted).AssemblyQualifiedName,
                    EventHandlerType = typeof(OrderSubmittedEventHandler).AssemblyQualifiedName
                });
            subscriptionEngine.Subscribe(new Subscription
            {
                EventType = typeof(ProductCreated).AssemblyQualifiedName,
                EventHandlerType = typeof(ProductCreatedEventHandler).AssemblyQualifiedName
            });

            return subscriptionEngine;
        }

       
    }
}