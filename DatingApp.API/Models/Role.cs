using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace DatingApp.API.Models
{
    public class Role : IdentityRole<int>
    {
        // navigation properties to connect users, roles and userroles
        public ICollection<UserRole> UserRoles { get; set; }
    }
}