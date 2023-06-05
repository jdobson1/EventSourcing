using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Projections;
using Products.Common.Queries;
using Products.Query.Projections.Views;

namespace Products.Query.Functions.Queries
{
    public class GetProductsFunction
    {
        private readonly ITenantViewRepository _viewRepository;

        public GetProductsFunction(ITenantViewRepository viewRepository)
        {
            _viewRepository = viewRepository;
        }

        [FunctionName("GetProducts")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Retrieving products...");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var getProductsQuery = JsonConvert.DeserializeObject<GetProducts>(requestBody);

            var productsView = await _viewRepository.LoadViewAsync(getProductsQuery.ClientId, nameof(ProductsView));
            var productView = JsonConvert.DeserializeObject<ProductsView>(productsView.Payload.ToString());

            return new OkObjectResult(productView.Products);
        }
    }
}
