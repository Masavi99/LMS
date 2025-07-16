using Microsoft.AspNetCore.Identity;

namespace LMS.Models.DBModels
{
    public class ApplicationUser : IdentityUser //inherit identity user
    {
        public string? FullName { get; set; }
    }
}
