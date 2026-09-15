using Microsoft.EntityFrameworkCore;
using Reclevia.Core.Tenancy;

namespace Reclevia.Api.Persistence;

public class RecleviaDbContext : DbContext
{
    public RecleviaDbContext(DbContextOptions<RecleviaDbContext> options) : base(options)
    { }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecleviaDbContext).Assembly);
    }
}
