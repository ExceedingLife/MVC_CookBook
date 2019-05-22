using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MVC_CookBook.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public int IId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime DateCreated { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}

/*
 * 
 * 
 * 
 * // Create Seeded Roles.
            context.Roles.AddOrUpdate(r => r.Id,
                new IdentityRole { Name = "Admin" },
                new IdentityRole { Name = "User" },
                new IdentityRole { Name = "UserPlus" },
                new IdentityRole { Name = "Developer" }
                );

            // Create Seeded Users
            if (!context.Users.Any(u => u.UserName == "Test"))
            {
                var store = new UserStore<ApplicationUser>(context);
                var manager = new UserManager<ApplicationUser>(store);
                string hash = manager.PasswordHasher.HashPassword("mmmmmm");
                var user = new ApplicationUser
                {
                    UserName = "Test",
                    Email = "Test@email.com",
                    DateCreated = DateTime.Now,
                    Birthday = DateTime.Now.AddYears(-99),
                    FirstName = "Bonald"
                };
                manager.Create(user, hash);
                manager.AddToRole(user.Id, "Developer");
            }
 * 
 * 
 * 
 */
