
using Products.Common.Dtos;
using System.Collections.Generic;

namespace Products.Query.Projections.Views
{
    public class ProductsView
    {
       public List<ProductDto> Products { get; set; } = new List<ProductDto>();
    }
}
