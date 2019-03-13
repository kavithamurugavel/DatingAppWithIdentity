using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace DatingApp.API.Data
{
    public class Seed
    {
        // https://docs.microsoft.com/en-us/previous-versions/aspnet/dn468199(v%3dvs.108)
        private readonly UserManager<User> _userManager;
        // https://docs.microsoft.com/en-us/previous-versions/aspnet/dn468201(v%3Dvs.108)
        // explanations of all the userManager and roleManager methods used in the below code can be also found in the links above
        private readonly RoleManager<Role> _roleManager;
        public Seed(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // serializing the user seeding json to object
        public void SeedUsers()
        {
            // making sure we don't keep accidentally seeding same users again and again
            if (!_userManager.Users.Any())
            {
                var userData = File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                var roles = new List<Role>()
                {
                    new Role{Name = "Member"},
                    new Role{Name = "Admin"},
                    new Role{Name = "Moderator"},
                    new Role{Name = "VIP"}
                };

                foreach(var role in roles)
                {
                    _roleManager.CreateAsync(role).Wait();
                }


                foreach (var user in users)
                {
                    // the initial photos that we are seeding are approved. Only subsequent photos should go thru approval
                    user.Photos.SingleOrDefault().IsApproved = true;
                    // we use Wait for the async (alternative to specifying method as async and awaiting the foll.line)
                    // Wait will synchronously block until the task completes. So the current thread is literally blocked waiting for the task to complete.
                    // https://stackoverflow.com/questions/13140523/await-vs-task-wait-deadlock
                    _userManager.CreateAsync(user, "password").Wait();
                    _userManager.AddToRoleAsync(user, "Member").Wait();
                }

                var adminUser = new User()
                {
                    UserName = "Admin"
                };

                // represents the result of an identity operation
                // https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.identityresult?view=aspnetcore-2.2
                IdentityResult result = _userManager.CreateAsync(adminUser, "password").Result;

                if(result.Succeeded)
                {
                    var admin = _userManager.FindByNameAsync("Admin").Result;
                    // adding the admin user created above to two roles admin and moderator
                    _userManager.AddToRolesAsync(admin, new[] {"Admin", "Moderator"}).Wait();
                }
            }

        }
    }
}