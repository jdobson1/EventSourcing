namespace ShoppingCart.Common.Commands
{
    public class AddItemToCart
    {
        public Guid ProductId { get; set; }
        public string ClientId { get; set; }
        public Guid CartId { get; set; }
        public int Quantity { get; set; }
    }
}