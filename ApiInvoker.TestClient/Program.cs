using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace ApiInvoker.TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // set up DI for a console app           
            var services = new ServiceCollection();

            services.AddApiClient<IProductService>("https://localhost:44318/api/product", true);

            var serviceProvider = services.BuildServiceProvider();



            //grab your injected proxy 
            var api = serviceProvider.GetService<IProductService>();

            // it's a console app, so we're running these calls synchrously 

            #region Gets

            Console.WriteLine("GET - Parameterless - GetProducts:");
            Dump( api.GetProducts() );

            Console.WriteLine("GET - Attribute Template - GetProduct");
            Dump( api.GetProduct(1) );

            Console.WriteLine("GET - FromQuery - SearchDescriptions");
            Dump( api.SearchDescriptions("basket") );

            Console.WriteLine("GET - Both, for whatever reason - SearchWithQualifier");
            Dump(api.SearchWithQualifier("basket", true));

            #endregion

            #region Posts

            var corn = new Product { Name = "Corn", Description = "Unshucked", Active = true };

            Console.WriteLine("POST - Body - AddProduct");
            Dump(api.AddProduct(corn));

            Console.WriteLine("POST - Form - AddProductUpload");
            Dump(api.AddProductUpload(corn));

            Console.WriteLine("POST - Body but with query param, for whatever reason - AddAtSpecificId");
            Dump(api.AddAtSpecificId(corn, 99));

            #endregion

            #region PUT

            var parsnip = api.GetProduct(3);
            parsnip.Description = "They're great!";

            Console.WriteLine("PUT - Body - UpdateProduct");
            Dump(api.UpdateProduct(parsnip));

            Console.WriteLine("PUT - Form - UpdateProductUpload");
            Dump(api.UpdateProductUpload(parsnip));

            #endregion

            #region Deletes

            Console.WriteLine("DELETE - Parameterless - RemoveAll:");
            Dump(api.RemoveAll());

            Console.WriteLine("DELETE - FromQuery - RemoveProduct");
            Dump(api.RemoveProduct(2));

            #endregion            


        }

        private static void Dump(Product p) => Console.WriteLine(p);
        private static void Dump(IEnumerable<Product> products)
        {
            foreach (var p in products)
                Console.WriteLine(p);
        }


    }
}
