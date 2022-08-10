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
    public class CreateProductFunction
    {
        private readonly IRepository<Product> _repository;
        public CreateProductFunction(IRepository<Product> repository)
        {
            _repository = repository;
        }

        [FunctionName("CreateProduct")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Creating product...");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            CreateProduct command = JsonConvert.DeserializeObject<CreateProduct>(requestBody);

            var product = new Product(command.Id, command.Name);
            await _repository.Save(product);

            return new OkObjectResult("Product created successfully!");
        }
    }
}
