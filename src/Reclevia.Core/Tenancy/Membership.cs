namespace Reclevia.Core.Tenancy;

public class Membership
{
    private Membership()
    { }

    public Membership(
        Guid tenantId,
        Guid userId,
        TenantRole role)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(
                nameof(role),
                role,
                "Invalid role value.");
        }

        TenantId = tenantId;
        UserId = userId;
        Role = role;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public TenantRole Role { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
}
