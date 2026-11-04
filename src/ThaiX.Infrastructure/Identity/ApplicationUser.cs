using Microsoft.AspNetCore.Identity;

namespace ThaiX.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public bool EmailOtpLoginEnabled { get; set; }
}
