namespace MVC_CookBook.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using MVC_CookBook.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<MVC_CookBook.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(MVC_CookBook.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.

            // Create Seeded Roles.
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
                    FirstName = "Bonald",
                    IId = 0
                };
                manager.Create(user, hash);
                manager.AddToRole(user.Id, "Developer");
            }
            
        }
    }
}
