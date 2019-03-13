using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace DatingApp.API.Models
{
    // IdentityRole: Represents a role in the identity system
    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.identityrole-1?view=aspnetcore-1.1
    public class Role : IdentityRole<int>
    {
        // navigation properties to connect users, roles and userroles
        public ICollection<UserRole> UserRoles { get; set; }
    }
}