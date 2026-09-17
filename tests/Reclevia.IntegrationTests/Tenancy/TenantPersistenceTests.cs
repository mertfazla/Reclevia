using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reclevia.Api.Persistence;
using Reclevia.Core.Tenancy;
using Reclevia.IntegrationTests.Infrastructure;

namespace Reclevia.IntegrationTests.Tenancy;

public sealed class TenantPersistenceTests
    : IClassFixture<RecleviaWebApplicationFactory>
{
    private readonly RecleviaWebApplicationFactory _factory;

    public TenantPersistenceTests(
        RecleviaWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SaveTenant_PersistsTenantInPostgreSql()
    {
        await ApplyMigrationsAsync();

        var tenant = new Tenant(
            $"Integration Test {Guid.NewGuid():N}");

        try
        {
            await using (var writeScope =
                _factory.Services.CreateAsyncScope())
            {
                var dbContext = writeScope.ServiceProvider
                    .GetRequiredService<RecleviaDbContext>();

                dbContext.Tenants.Add(tenant);
                await dbContext.SaveChangesAsync();
            }

            // A new scope proves that the tenant is read from PostgreSQL,
            // not from EF Core's in-memory change tracker.
            await using var readScope =
                _factory.Services.CreateAsyncScope();

            var readDbContext = readScope.ServiceProvider
                .GetRequiredService<RecleviaDbContext>();

            var savedTenant = await readDbContext.Tenants
                .AsNoTracking()
                .SingleAsync(saved => saved.Id == tenant.Id);

            Assert.Equal(tenant.Id, savedTenant.Id);
            Assert.Equal(tenant.Name, savedTenant.Name);
        }
        finally
        {
            await DeleteTenantAsync(tenant.Id);
        }
    }

    private async Task ApplyMigrationsAsync()
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<RecleviaDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    private async Task DeleteTenantAsync(Guid tenantId)
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<RecleviaDbContext>();

        var tenant = await dbContext.Tenants.FindAsync(tenantId);

        if (tenant is null)
        {
            return;
        }

        dbContext.Tenants.Remove(tenant);
        await dbContext.SaveChangesAsync();
    }
}
