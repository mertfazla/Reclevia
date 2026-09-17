using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Reclevia.IntegrationTests.Infrastructure;

namespace Reclevia.IntegrationTests.Health
{
    public class HealthEndpointTests : IClassFixture<RecleviaWebApplicationFactory>
    {
        private readonly RecleviaWebApplicationFactory _factory;

        public HealthEndpointTests(RecleviaWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetLiveness_ReturnsHealthy()
        {
            using var client = CreateClient();

            using var response = await client.GetAsync("/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(
                "Healthy",
                await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task GetReadiness_ReturnsHealthy_WhenPostgresIsAvailable()
        {
            using var client = CreateClient();

            using var response = await client.GetAsync("/health/ready");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(
                "Healthy",
                await response.Content.ReadAsStringAsync());
        }

        private HttpClient CreateClient()
        {
            return _factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri("https://localhost"),
                    AllowAutoRedirect = false
                });
        }
    }
}
