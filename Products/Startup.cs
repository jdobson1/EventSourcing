using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using EventStore;
using Products.Infrastructure.Repositories;
using Core.Domain;
using Projections;
using Subscriptions;
using Products.Domain;
using Orders.Common.Events;

[assembly: FunctionsStartup(typeof(Products.Startup))]

namespace Products
{
    public class Startup : FunctionsStartup, IEventTypeResolver
    {
        private static string EndpointUrl = Environment.GetEnvironmentVariable("CosmosEndpointUrl"); //"https://cosmos-es.documents.azure.com:443/";
        private static readonly string AuthorizationKey = Environment.GetEnvironmentVariable("CosmosAuthorizationKey"); //"Pq3WO6OdB6yaryG6AdQgGleV0q210Eu60coHoyvJMhTH0abvE5EgEmywagoGv41rOLbcnpRSQHtRHowmwPQdhA==";
        private static string DatabaseId = Environment.GetEnvironmentVariable("CosmosEventStoreDatabaseId"); //"event-store";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            //builder.Services.AddHttpClient();

            builder.Services.AddTransient<IRepository<Product>, ProductRepository>();
            builder.Services.AddSingleton<IEventStore>((s) =>
            {
                return new CosmosEventStore(this, EndpointUrl, AuthorizationKey, DatabaseId);
            });
            //builder.Services.AddSingleton((s) =>
            //{
            //    return InitializeSubscriptions();
            //});

            // builder.Services.AddSingleton<ILoggerProvider, MyLoggerProvider>();

            //todo: add database initializer to add collections for events and snapshots
            //Task.Run(async () => await InitializeSubscriptions());
        }

        public Type GetEventType(string typeName)
        {
            return Type.GetType($"Products.Common.Events.{typeName}, Products.Common");
        }

        //private ISubscriptionEngine InitializeSubscriptions()
        //{
        //    ISubscriptionEngine subscriptionEngine = new SubscriptionEngine(this, EndpointUrl, "products", AuthorizationKey, DatabaseId);
        //    subscriptionEngine.Subscribe(
        //        new Subscription 
        //        { 
        //            EventType = typeof(OrderSubmitted).AssemblyQualifiedName, 
        //            EventHandlerType = typeof(CategoryCreatedEventHandler).AssemblyQualifiedName 
        //        });

        //    return subscriptionEngine;
        //}
    }
}