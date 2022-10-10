using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ShoppingCart.Common.Commands;
using Core.Domain;

namespace ShoppingCart.Functions
{
    public class AddItemToCartFunction
    {
        private readonly IRepository<Domain.ShoppingCart> _repository;

        public AddItemToCartFunction(IRepository<Domain.ShoppingCart> repository)
        {
            _repository = repository;   
        }

        [FunctionName("AddItemToCart")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Adding item to cart...");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var command = JsonConvert.DeserializeObject<AddItemToCart>(requestBody);

            var shoppingCart = new Domain.ShoppingCart(command.CartId, command.ClientId);
            shoppingCart.AddItem(command.ProductId, command.Quantity);
            await _repository.Save(shoppingCart, command.ClientId);

            return new OkObjectResult("Item added to cart successfully!");
        }
    }
}
