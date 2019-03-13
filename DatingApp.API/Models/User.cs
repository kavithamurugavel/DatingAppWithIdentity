using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace DatingApp.API.Models
{
    //Each User can have many associated Roles, and each Role can be associated with many Users. 
    // This is a many-to-many relationship that requires a join table in the database. 
    // The join table is represented by the UserRole entity.
    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.entityframeworkcore.identityuser-1?view=aspnetcore-1.1
    public class User : IdentityUser<int>
    {
        // the properties like ID, Username, Password salt and hash are already defined in 
        // IdentityUser, so we remove those that we created earlier
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public ICollection<Photo> Photos { get; set; }
        public ICollection<Like> Likers { get; set; }
        public ICollection<Like> Likees { get; set; }
        public ICollection<Message> MessagesSent { get; set; }
        public ICollection<Message> MessagesReceived { get; set; }
        
        // Earlier Asp.Net identity included a navigation property for user roles as well.
        // They have since removed that navigation property. So we need to use these
        // properties as below.
        public ICollection<UserRole> UserRoles { get; set; }
    }
}