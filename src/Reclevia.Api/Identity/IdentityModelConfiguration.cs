using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Reclevia.Api.Identity;

public static class IdentityModelConfiguration
{
    public static void ConfigureIdentityTables(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>()
            .ToTable("users", "identity");

        modelBuilder.Entity<IdentityRole<Guid>>()
            .ToTable("roles", "identity");

        modelBuilder.Entity<IdentityUserRole<Guid>>()
            .ToTable("user_roles", "identity");

        modelBuilder.Entity<IdentityUserClaim<Guid>>()
            .ToTable("user_claims", "identity");

        modelBuilder.Entity<IdentityUserLogin<Guid>>()
            .ToTable("user_logins", "identity");

        modelBuilder.Entity<IdentityUserToken<Guid>>()
            .ToTable("user_tokens", "identity");

        modelBuilder.Entity<IdentityRoleClaim<Guid>>()
            .ToTable("role_claims", "identity");
    }
}
