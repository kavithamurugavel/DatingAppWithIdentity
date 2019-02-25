using Microsoft.AspNetCore.Identity;

namespace DatingApp.API.Models
{
    public class UserRole : IdentityUserRole<int>
    {
        // navigation properties to connect users, roles and userroles
        public User User { get; set; }
        public Role Role { get; set; }
    }
}