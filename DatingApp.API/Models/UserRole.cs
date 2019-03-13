using Microsoft.AspNetCore.Identity;

namespace DatingApp.API.Models
{
    // IdentityUserRole represents the link between a user and a role.
    // here int is the type of primary key used tfor users and roles
    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.identityuserrole-1?view=aspnetcore-1.1
    public class UserRole : IdentityUserRole<int>
    {
        // navigation properties to connect users, roles and userroles
        public User User { get; set; }
        public Role Role { get; set; }
    }
}