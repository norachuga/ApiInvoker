using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ApiInvoker.TestClient
{
    public interface IProductService
    {
        // GET------
        [HttpGet]
        public IEnumerable<Product> GetProducts();

        [HttpGet("{id}")]
        public Product GetProduct(int id);

        [HttpGet]
        public IEnumerable<Product> SearchDescriptions([FromQuery] string query);

        [HttpGet("{active}")]
        public IEnumerable<Product> SearchWithQualifier([FromQuery] string query, bool active);

        // POST-----

        [HttpPost]
        public Product AddProduct([FromBody] Product product);

        [HttpPost]
        public Product AddProductUpload([FromForm] Product product);

        [HttpPost("{id}")]
        public Product AddAtSpecificId([FromForm] Product product, int id);

        // PUT-----

        [HttpPut]
        public Product UpdateProduct([FromBody] Product product);

        [HttpPut]
        public Product UpdateProductUpload([FromForm] Product product);

        // DELETE---

        [HttpDelete]
        public IEnumerable<Product> RemoveAll();

        [HttpDelete("{id}")]
        public Product RemoveProduct(int id);

    }
}
