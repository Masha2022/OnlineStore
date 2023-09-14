using Microsoft.AspNetCore.Identity;

namespace OnlineStore.Models;

public class ApplicationUser: IdentityUser
{
    public string FullName { get; set; }
}