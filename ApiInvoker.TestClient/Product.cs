using System;
using System.Collections.Generic;
using System.Text;

namespace ApiInvoker.TestClient
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }

        public override string ToString()
        {
            string desc = Active ? Description : "[inactive product]";
            return $"{Id} | {Name}\t| {desc}";
        }
    }
}
