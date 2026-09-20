using Microsoft.AspNetCore.Identity;

namespace Reclevia.Api.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
        Id = Guid.NewGuid();
    }
}
