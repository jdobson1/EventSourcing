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
        private static string EndpointUrl = Environment.GetEnvironmentVariable("CosmosEndpointUrl"); //"https://cosmos-es.documents.azure.com:443/";
        private static readonly string AuthorizationKey = Environment.GetEnvironmentVariable("CosmosAuthorizationKey"); //"Pq3WO6OdB6yaryG6AdQgGleV0q210Eu60coHoyvJMhTH0abvE5EgEmywagoGv41rOLbcnpRSQHtRHowmwPQdhA==";
        private static string DatabaseId = Environment.GetEnvironmentVariable("CosmosEventStoreDatabaseId"); //"event-store";

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
            var type = Type.GetType($"Orders.Common.Events.{typeName}, Orders.Common");

            return type ?? Type.GetType($"ShoppingCart.Common.Events.{typeName}, ShoppingCart.Common");

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

        private IProjectionEngine InitializeProjectionEngine(IViewRepository viewRepository)
        {
            var projectionEngine = new ProjectionEngine(this, viewRepository);

            projectionEngine.RegisterProjection(new ShoppingCartProjection());

            return projectionEngine;
        }
    }
}