using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DataContext: IdentityDbContext<User, Role, int, 
    IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, 
    IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        // the foll.lines of code are part of normal DBContext code in an ASP.NET Web App, say. Unsure of another way to add this code other than physically type it out in a ASP.NET Web API
        public DataContext(DbContextOptions<DataContext> options): base (options){}

        public DbSet<Value> Values { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        // overriding the OnModelCreating method, based on Fluent API (basically a means to define 
        // custom code-first conventions with EF), for our like functionality Sec 15 Lec 150
        // https://docs.microsoft.com/en-us/ef/ef6/modeling/code-first/fluent/types-and-properties
        // https://docs.microsoft.com/en-us/ef/ef6/modeling/code-first/conventions/custom
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // the foll. configures the schema for the Identity framework
            base.OnModelCreating(builder);

            // config for Identity 
            builder.Entity<UserRole>(userRole => 
            {
                userRole.HasKey(ur => new {ur.UserId, ur.RoleId});

                userRole.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

                userRole.HasOne(ur => ur.User)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
            });

            // making our composite primary key a combination of liker and likee ID, since we 
            // don't want one user liking another user more than once
            builder.Entity<Like>()
            .HasKey(k => new {k.LikerID, k.LikeeID});

            // user can like many users and also be liked by many users
            builder.Entity<Like>()
            .HasOne(u => u.Likee)
            .WithMany(u => u.Likers)
            .HasForeignKey(u => u.LikeeID) // going back to users table to get the user's ID that corresponds to the LikeeID
            .OnDelete(DeleteBehavior.Restrict); // we don't want deletion of a like be a cascading deletion of a user

            builder.Entity<Like>()
            .HasOne(u => u.Liker)
            .WithMany(u => u.Likees)
            .HasForeignKey(u => u.LikerID)
            .OnDelete(DeleteBehavior.Restrict);

            // custom configuring the messages entity next
            // this is basically explaining to EF that one sender has many sent messages
            builder.Entity<Message>()
            .HasOne(u => u.Sender)
            .WithMany(m => m.MessagesSent)
            .OnDelete(DeleteBehavior.Restrict);

            // this is basically explaining to EF that one receiver has many received messages
            builder.Entity<Message>()
            .HasOne(u => u.Recipient)
            .WithMany(m => m.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);

            // filtering out the approved photos using global query filters
            // this is so that other users cannot see a particular user's unapproved photos
            // https://docs.microsoft.com/en-us/ef/core/querying/filters
            builder.Entity<Photo>().HasQueryFilter(p => p.IsApproved);
        }
    }
}