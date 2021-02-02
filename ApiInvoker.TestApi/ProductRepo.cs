using System.Collections.Generic;

namespace ApiInvoker.TestApi
{
    public  static class ProductRepo
    {
        public static IEnumerable<Product> Products = new List<Product>
        {
            new Product {
                Id = 1,
                Name = "Pumpkin",
                Description = "decorative and edible",
                Active = true
            },
            new Product {
                Id = 2,
                Name = "Blueberry",
                Description = "basket of blueberries",
                Active = true
            },
            new Product {
                Id = 3,
                Name = "Parsnip",
                Description = "white, undesirable",
                Active = true
            },
            new Product {
                Id = 4,
                Name = "Green Bean",
                Description = "basket of the green kind",
                Active = false
            },
        };
    }
}
