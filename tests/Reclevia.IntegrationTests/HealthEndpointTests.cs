using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace Reclevia.IntegrationTests
{
    public class HealthEndpointTests
    {
        [Fact]
        public async Task GetHealth_ReturnHealthy()
        {
            using var factory = new WebApplicationFactory<Program>();

            using var client = factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri("https://localhost"),
                    AllowAutoRedirect = false
                });

            using var response = await client.GetAsync("/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal("Healthy", content);
        }
    }
}
