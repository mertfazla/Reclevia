using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Reclevia.Api.Identity;
using Reclevia.Core.Tenancy;

namespace Reclevia.Api.Persistence;

public class RecleviaDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public RecleviaDbContext(DbContextOptions<RecleviaDbContext> options) : base(options)
    { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Membership> Memberships => Set<Membership>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureIdentityTables();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecleviaDbContext).Assembly);
    }
}
