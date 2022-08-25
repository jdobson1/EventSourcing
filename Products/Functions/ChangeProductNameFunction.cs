using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Products.Common.Commands;
using Core.Domain;
using Products.Domain;

namespace Products.Functions
{
    public class ChangeProductNameFunction
    {
        private readonly IRepository<Product> _repository;
        public ChangeProductNameFunction(IRepository<Product> repository)
        {
            _repository = repository;
        }

        [FunctionName("ChangeProductName")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Changing product name...");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var command = JsonConvert.DeserializeObject<ChangeProductName>(requestBody);

            var product = await _repository.GetById(command.ProductId);
            product.Name = command.ProductName;
            await _repository.Save(product);

            return new OkObjectResult("Product name changed successfully!");
        }
    }
}
