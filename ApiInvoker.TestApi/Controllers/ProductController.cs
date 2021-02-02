using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace ApiInvoker.TestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private static IEnumerable<Product> _repo = ProductRepo.Products;

        [HttpGet]
        public IEnumerable<Product> GetProducts()
        {
            return _repo;
        }

        [HttpGet("{id}")]
        public Product GetProduct(int id)
        {
            return _repo.SingleOrDefault(x => x.Id == id);
        }

        [HttpGet]
        public IEnumerable<Product> SearchDescriptions([FromQuery]string query)
        {
            return _repo.Where(x => x.Description.Contains(query));
        }

        [HttpGet("{active}")]
        public IEnumerable<Product> SearchWithQualifier([FromQuery] string query, bool active)
        {
            return _repo.Where(x => x.Active == active && x.Description.Contains(query));
        }

        [HttpPost]
        public Product AddProduct([FromBody]Product product)
        {
            product.Id = _repo.Max(x => x.Id) + 1;
            return product;
        }

        [HttpPost]
        public Product AddProductUpload([FromForm]Product product)
        {
            product.Id = _repo.Max(x => x.Id) + 1;
            return product;
        }

        [HttpPost("{id}")]
        public Product AddAtSpecificId([FromForm] Product product, int id)
        {
            product.Id = id;
            return product;
        }

        [HttpPut]
        public Product UpdateProduct([FromBody] Product product)
        {
            return product;
        }

        [HttpPut]
        public Product UpdateProductUpload([FromForm] Product product)
        {
            return product;
        }


        [HttpDelete]
        public IEnumerable<Product> RemoveAll()
        {
            var all = _repo;
            foreach (var p in all)
                p.Active = false;

            return all;
        }

        [HttpDelete("{id}")]
        public Product RemoveProduct(int id)
        {
            var product = GetProduct(id);
            product.Active = false;
            return product;
        }

    }
}
