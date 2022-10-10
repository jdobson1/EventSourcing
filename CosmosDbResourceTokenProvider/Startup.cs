using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using Core.Infrastructure;

[assembly: FunctionsStartup(typeof(CosmosDbResourceTokenProvider.Startup))]

namespace CosmosDbResourceTokenProvider
{
    public class Startup : FunctionsStartup
    {
        public static readonly string EndpointUrl = Environment.GetEnvironmentVariable("CosmosEndpointUrl");
        public static readonly string AuthorizationKey = Environment.GetEnvironmentVariable("CosmosAuthorizationKey");

        public override void Configure(IFunctionsHostBuilder builder)
        {
            
            builder.Services.AddSingleton<ICosmosClientFactory>(s => new CosmosClientFactory());

            // builder.Services.AddSingleton<ILoggerProvider, MyLoggerProvider>();
        }
    }
}