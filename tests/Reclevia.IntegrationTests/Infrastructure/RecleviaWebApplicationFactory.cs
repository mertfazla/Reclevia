using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Reclevia.IntegrationTests.Infrastructure;

public sealed class RecleviaWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            var configuration = configurationBuilder.Build();

            var testConnectionString =
                configuration.GetConnectionString("RecleviaTests");

            if (string.IsNullOrWhiteSpace(testConnectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'RecleviaTests' is missing.");
            }

            // The application expects "Reclevia", but integration tests
            // must use the isolated test database.
            configurationBuilder.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Reclevia"] = testConnectionString
                });
        });
    }
}
