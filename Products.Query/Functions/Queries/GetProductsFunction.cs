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
using System.Collections.Generic;
using Products.Common.Dtos;

namespace Products.Query.Functions.Queries
{
    public class GetProductsFunction
    {
        private readonly IViewRepository _viewRepository;

        public GetProductsFunction(IViewRepository viewRepository)
        {
            _viewRepository = viewRepository;
        }

        [FunctionName("GetProducts")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Retrieving products...");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            GetProducts getProductsQuery = JsonConvert.DeserializeObject<GetProducts>(requestBody);
            var productsView = await _viewRepository.LoadViewAsync(nameof(ProductsView));
            var productView = JsonConvert.DeserializeObject<ProductsView>(productsView.Payload.ToString());
            return new OkObjectResult(productView.Products);
        }
    }
}
