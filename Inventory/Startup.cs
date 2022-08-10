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

[assembly: FunctionsStartup(typeof(Inventory.Startup))]

namespace Inventory
{
    public class Startup : FunctionsStartup, IEventTypeResolver
    {
        private static string EndpointUrl = Environment.GetEnvironmentVariable("CosmosEndpointUrl"); //"https://cosmos-es.documents.azure.com:443/";
        private static readonly string AuthorizationKey = Environment.GetEnvironmentVariable("CosmosAuthorizationKey"); //"Pq3WO6OdB6yaryG6AdQgGleV0q210Eu60coHoyvJMhTH0abvE5EgEmywagoGv41rOLbcnpRSQHtRHowmwPQdhA==";
        private static string DatabaseId = Environment.GetEnvironmentVariable("CosmosEventStoreDatabaseId"); //"event-store";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            //builder.Services.AddHttpClient();

            builder.Services.AddTransient<IRepository<ProductInventory>, ProductRepository>();
            builder.Services.AddSingleton<IEventStore>((s) =>
            {
                return new CosmosEventStore(this, EndpointUrl, AuthorizationKey, DatabaseId);
            });
            builder.Services.AddSingleton((s) =>
            {
                return InitializeSubscriptions();
            });
           

            // builder.Services.AddSingleton<ILoggerProvider, MyLoggerProvider>();

            //todo: add database initializer to add collections for events and snapshots
            //Task.Run(async () => await InitializeSubscriptions());
        }

        public Type GetEventType(string typeName)
        {
            var type = Type.GetType($"Inventory.Common.Events.{typeName}, Inventory.Common");

            if (type != null) return type;

            type = Type.GetType($"Orders.Common.Events.{typeName}, Orders.Common");

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

            return subscriptionEngine;
        }

       
    }
}