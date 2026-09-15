using Reclevia.Core.Tenancy;
using Xunit;

namespace Reclevia.UnitTests.Tenancy;

public sealed class TenantTests
{
    [Fact]
    public void Constructor_TrimsName()
    {
        var tenant = new Tenant("  Northstar Consulting  ");

        Assert.Equal("Northstar Consulting", tenant.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\r\n")]
    public void Constructor_RejectsBlankName(string name)
    {
        Assert.Throws<ArgumentException>(() => new Tenant(name));
    }

    [Fact]
    public void Constructor_RejectsNameLongerThanLimit()
    {
        var name = new string('A', Tenant.MaxNameLength + 1);

        Assert.Throws<ArgumentException>(() => new Tenant(name));
    }

    [Fact]
    public void Constructor_AcceptsNameAtLimit()
    {
        var name = new string('A', Tenant.MaxNameLength);

        var tenant = new Tenant(name);

        Assert.Equal(name, tenant.Name);
    }
}
