using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ASPNETCoreDapperRLS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController: ControllerBase
    {

        [HttpGet]
        public async Task<IEnumerable<Product>> GetAll()
        {
            var connection = (SqlConnection)HttpContext.Items["TenantConnection"]; // HttpContext not available in constructor
            return await connection.QueryAsync<Product>("SELECT * FROM Product");
        }


        [HttpGet("{productId}", Name = "ProductGet")]
        public async Task<ActionResult<Product>> GetById(Guid productId)
        {
            var connection = (SqlConnection)HttpContext.Items["TenantConnection"];
            var product = await connection.QueryFirstOrDefaultAsync<Product>("SELECT * FROM Product WHERE ProductId = @ProductId", new { ProductId = productId });
            if (product == null) return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> Post([FromBody]Product product)
        {
            var connection = (SqlConnection)HttpContext.Items["TenantConnection"];
            var tenant = (Tenant)HttpContext.Items["Tenant"];
            product.ProductId = Guid.NewGuid();
            product.TenantId = tenant.TenantId;
            await connection.ExecuteAsync(@"INSERT INTO Product(ProductID, TenantId, ProductName, UnitPrice, UnitsInStock, ReorderLevel, Discontinued) 
                                            VALUES(@ProductID, @TenantId, @ProductName, @UnitPrice, @UnitsInStock, @ReorderLevel, @Discontinued)",
                                            product);

            var url = Url.Link("ProductGet", new { productId = product.ProductId });
            return Created(url, product);
        }
    }
}
