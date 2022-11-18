
using Products.Common.Dtos;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Projections;

namespace Products.Query.Projections.Views
{
    public class ProductsView : IView
    {
       public List<ProductDto> Products { get; set; } = new List<ProductDto>();
       public JObject Payload { get; set; } = new JObject();
    }
}
