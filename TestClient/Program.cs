// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using Products.Common.Dtos;
using System.Text;

Console.Write($"{Environment.NewLine}Press any key to create product...");
Console.ReadKey(true);
var httpClient = new HttpClient();
var productId = Guid.NewGuid();
var createProductCommand = new { Id = productId, Name = $"test-{productId}" };
var content = new StringContent(JsonConvert.SerializeObject(createProductCommand), Encoding.UTF8, "application/json");
await httpClient.PostAsync("http://localhost:7179/api/CreateProduct", content);

Console.Write($"{Environment.NewLine}Press any key to update product name...");
Console.ReadKey(true);

var changeProductNameCommand = new { ProductId = productId, ProductName = $"updated-{productId}" };
content = new StringContent(JsonConvert.SerializeObject(changeProductNameCommand), Encoding.UTF8, "application/json");
await httpClient.PostAsync("http://localhost:7179/api/ChangeProductName", content);

Console.Write($"{Environment.NewLine}Press any key to get products...");
Console.ReadKey(true);

var getProductsQuery = new { };
content = new StringContent(JsonConvert.SerializeObject(getProductsQuery), Encoding.UTF8, "application/json");
var response = await httpClient.PostAsync("http://localhost:7073/api/GetProducts", content);
string responseBody = await response.Content.ReadAsStringAsync();
Console.Write(responseBody);

var products = JsonConvert.DeserializeObject<List<ProductDto>>(responseBody);

Console.Write($"{Environment.NewLine}Press any key to add item to cart...");
Console.ReadKey(true);

var addItemToCart = new { productId = products.First().Id, cartId = Guid.NewGuid(), quantity = 3 };
content = new StringContent(JsonConvert.SerializeObject(addItemToCart), Encoding.UTF8, "application/json");
await httpClient.PostAsync("http://localhost:7072/api/AddItemToCart", content);

Console.Write($"{Environment.NewLine}Press any key to exit...");
Console.ReadKey(true);

