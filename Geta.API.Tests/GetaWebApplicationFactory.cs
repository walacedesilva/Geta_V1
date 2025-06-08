using Geta.API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace Geta.API.Tests;

public class GetaWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove a configuração do DbContext da aplicação original.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Adiciona um novo DbContext que usa um banco de dados em memória com um nome único (GUID).
            // Isso garante que cada classe de teste que usa esta factory obtenha seu próprio banco de dados,
            // prevenindo conflitos e garantindo o isolamento total dos testes.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase($"GetaTestDb-{System.Guid.NewGuid()}");
            });
        });
    }
}