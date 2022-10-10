using System;

namespace Products.Common.Commands
{
    public class ChangeProductName
    {
        public Guid ProductId {get; set;}
        public string ClientId { get; set; }
        public string ProductName {get; set;}
    }
}