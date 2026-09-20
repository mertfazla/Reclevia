using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reclevia.Api.Identity;
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
    public async Task SaveMembership_PersistsTenantUserRelationship()
    {
        await ApplyMigrationsAsync();

        var tenant = new Tenant(
            $"Membership Test {Guid.NewGuid():N}");

        var user = new ApplicationUser
        {
            UserName = $"user-{Guid.NewGuid():N}"
        };

        var membership = new Membership(
            tenant.Id,
            user.Id,
            TenantRole.Owner);

        try
        {
            await using (var writeScope =
                _factory.Services.CreateAsyncScope())
            {
                var dbContext = writeScope.ServiceProvider
                    .GetRequiredService<RecleviaDbContext>();

                dbContext.Tenants.Add(tenant);
                dbContext.Users.Add(user);
                dbContext.Memberships.Add(membership);
                await dbContext.SaveChangesAsync();
            }

            await using var readScope =
                _factory.Services.CreateAsyncScope();

            var readDbContext = readScope.ServiceProvider
                .GetRequiredService<RecleviaDbContext>();

            var savedMembership = await readDbContext.Memberships
                .AsNoTracking()
                .SingleAsync(saved =>
                    saved.TenantId == tenant.Id &&
                    saved.UserId == user.Id);

            Assert.Equal(TenantRole.Owner, savedMembership.Role);
        }
        finally
        {
            await DeleteMembershipDataAsync(
                tenant.Id,
                user.Id);
        }
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

    private async Task DeleteMembershipDataAsync(
        Guid tenantId,
        Guid userId)
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<RecleviaDbContext>();

        var membership = await dbContext.Memberships
            .FindAsync(tenantId, userId);

        if (membership is not null)
        {
            dbContext.Memberships.Remove(membership);
        }

        var tenant = await dbContext.Tenants.FindAsync(tenantId);
        if (tenant is not null)
        {
            dbContext.Tenants.Remove(tenant);
        }

        var user = await dbContext.Users.FindAsync(userId);
        if (user is not null)
        {
            dbContext.Users.Remove(user);
        }

        await dbContext.SaveChangesAsync();
    }
}
