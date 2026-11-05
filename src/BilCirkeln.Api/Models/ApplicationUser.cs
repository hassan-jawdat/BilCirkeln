using Microsoft.AspNetCore.Identity;

namespace BilCirkeln.Api.Models;

// Our user. IdentityUser already has Email, PasswordHash, etc.
// We only add the fields we need ourselves.
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public List<CarSubmission> CarSubmissions { get; set; } = new();
}
