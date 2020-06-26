using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASPNETCoreDapperRLS
{
    public class Product
    {
        public Guid ProductId { get; set; }
        public Guid TenantId { get; set; }
        public string ProductName { get; set; }
        public Decimal UnitPrice { get; set; }
        public Int16 UnitsInStock { get; set; }
        public Int16 UnitsOnOrder { get; set; }
        public Int16 ReorderLevel { get; set; }
        public bool Discontinued { get; set; }
    }
}
