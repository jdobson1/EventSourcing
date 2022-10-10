using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orders.Common.Commands;
using Core.Domain;
using Orders.Domain;
using Projections;
using System.Collections.Generic;
using Orders.Projections.Views;
using System.Linq;
using System;

namespace Orders.Functions
{
    public class SubmitOrderFunction
    {
        private readonly IRepository<Order> _repository;
        private readonly IViewRepository _viewRepository;
        public SubmitOrderFunction(IRepository<Order> repository, IViewRepository viewRepository)
        {
            _repository = repository;
            _viewRepository = viewRepository;
        }

        [FunctionName("SubmitOrder")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Submitting order...");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var command = JsonConvert.DeserializeObject<SubmitOrder>(requestBody);

            var order = new Order(
                command.ShoppingCartId, 
                command.FirstName, 
                command.LastName, 
                new Address(command.BillingAddress), 
                new Address(command.ShippingAddress), 
                await GetOrderItems(command.ShoppingCartId));

            await _repository.Save(order, command.ClientId);

            return new OkObjectResult("Order submitted successfully!");
        }

        private async Task<List<OrderItem>> GetOrderItems(Guid shoppingCartId)
        {
            var shoppingCartView = await _viewRepository.LoadViewAsync<ShoppingCartView>(nameof(ShoppingCartView));
            var shoppingCart = shoppingCartView.ShoppingCarts.SingleOrDefault(x => x.ShoppingCartId == shoppingCartId);

            var orderItems = new List<OrderItem>();
            if (shoppingCart?.Items == null) return orderItems;

            orderItems.AddRange(shoppingCart.Items.Select(shoppingCartItem => new OrderItem(shoppingCartItem.ProductId, shoppingCartItem.Quantity)));

            return orderItems;
        }
    }
}
