using Reclevia.Core.Tenancy;

namespace Reclevia.UnitTests.Tenancy;

public class MembershipTests
{
    [Fact]
    public void Constructor_CreatesMembership()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var membership = new Membership(
            tenantId,
            userId,
            TenantRole.Finance);

        Assert.Equal(tenantId, membership.TenantId);
        Assert.Equal(userId, membership.UserId);
        Assert.Equal(TenantRole.Finance, membership.Role);
        Assert.NotEqual(default, membership.CreatedAtUtc);
    }

    [Fact]
    public void Constructor_RejectsEmptyTenantId()
    {
        Assert.Throws<ArgumentException>(
            () => new Membership(
                Guid.Empty,
                Guid.NewGuid(),
                TenantRole.Viewer));
    }

    [Fact]
    public void Constructor_RejectsEmptyUserId()
    {
        Assert.Throws<ArgumentException>(
            () => new Membership(
                Guid.NewGuid(),
                Guid.Empty,
                TenantRole.Viewer));
    }

    [Fact]
    public void Constructor_RejectsInvalidRole()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Membership(
                Guid.NewGuid(),
                Guid.NewGuid(),
                (TenantRole)999));
    }
}
