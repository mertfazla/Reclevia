using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reclevia.Api.Identity;
using Reclevia.Core.Tenancy;

namespace Reclevia.Api.Persistence.Configurations;

public sealed class MembershipConfiguration
    : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("memberships", "tenancy");

        // A user can have only one membership in the same tenant.
        builder.HasKey(membership => new
        {
            membership.TenantId,
            membership.UserId
        });

        builder.Property(membership => membership.TenantId)
            .HasColumnName("tenant_id");

        builder.Property(membership => membership.UserId)
            .HasColumnName("user_id");

        builder.Property(membership => membership.Role)
            .HasColumnName("role")
            .IsRequired();

        builder.Property(membership => membership.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(membership => membership.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(membership => membership.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
