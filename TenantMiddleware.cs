using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace ASPNETCoreDapperRLS
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate next;
        
        public TenantMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, IConfiguration configuration)
        {
            context.Items["TenantConnection"] = null;
            context.Items["Tenant"] = null;
            var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
            if (string.IsNullOrEmpty(apiKey))
            {
                return;
            }
            Guid apiKeyGuid;
            if (!Guid.TryParse(apiKey, out apiKeyGuid))
            {
                return;
            }
            using (var connection = new SqlConnection(configuration["ConnectionStrings:DefaultConnection"]))
            {
                await connection.OpenAsync();
                var tenant = await SetTenant(connection, apiKeyGuid);
                context.Items["TenantConnection"] = connection;
                context.Items["Tenant"] = tenant;
                await next.Invoke(context);
            }
        }

        private async Task<Tenant> SetTenant(SqlConnection connection, Guid apiKey)
        {
            var tenant = await connection.QueryFirstOrDefaultAsync<Tenant>("SELECT * FROM Tenant WHERE APIKey = @APIKey", new { APIKey = apiKey });
            await connection.ExecuteAsync(@"EXEC dbo.sp_set_session_context @key = N'TenantId', @value = @value", new { value = tenant.TenantId });
            return tenant;
        }
    }

    public static class TenantMiddlewareExtension
    {
        public static IApplicationBuilder UseTenant(this IApplicationBuilder app)
        {
            app.UseMiddleware<TenantMiddleware>();
            return app;
        }
    }
}
