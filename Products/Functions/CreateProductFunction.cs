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
        public CreateProductFunction(IRepositorySnapShotDecorator<Product> repository)
        {
            _repository = repository;
        }

        [FunctionName("CreateProduct")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Creating product...");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var command = JsonConvert.DeserializeObject<CreateProduct>(requestBody);

            var product = new Product(command.Id, command.Name, command.ClientId);
            await _repository.Save(product, command.ClientId);

            return new OkObjectResult("Product created successfully!");
        }
    }
}
