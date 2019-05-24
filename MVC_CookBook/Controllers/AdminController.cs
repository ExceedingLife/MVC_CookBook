using System;
using System.Net;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using MVC_CookBook.Models;


namespace MVC_CookBook.Controllers
{

    public class AdminController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly ApplicationDbContext context;

        public AdminController()
        {
            context = new ApplicationDbContext();
        }
        public AdminController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            context = new ApplicationDbContext();
        }
        public ApplicationSignInManager SignInManager
        {
            get {  return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); }
            private set { _signInManager = value; }
        }
        public ApplicationUserManager UserManager
        {
            get {  return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }
        public IAuthenticationManager AuthManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }


        // GET: Admin/Dashboard/
        [HttpGet]
        public ActionResult Dashboard()
        {

            return View();
        }

        // GET: Admin/ViewAllUsers/
        [HttpGet]
        public async Task<ActionResult> ViewAllUsers()
        {
            var userDetails = await 
                              (from user in context.Users
                               from userrole in user.Roles
                               join role in context.Roles
                               on userrole.RoleId
                               equals role.Id
                               select new AdminUserViewModel()
                               {
                                   Id = user.IId,
                                   Guid = user.Id,
                                   FirstName = user.FirstName,
                                   LastName = user.LastName,
                                   DateCreated = user.DateCreated,
                                   Birthday = user.Birthday,
                                   UserName = user.UserName,
                                   Email = user.Email,
                                   UserRole = role.Name
                               }).ToListAsync();
            if(userDetails == null)
            {
                return HttpNotFound();
            }
            return View(userDetails);
        }

        //GET: Admin/CreateUser/
        [HttpGet]
        public async Task<ActionResult> AdminCreateUser()
        {
            var model = new RegisterViewModel()
            {
                Birthday = DateTime.Today.AddYears(-21)
            };

            ViewBag.Roles = new SelectList(await context.Roles.ToListAsync(), "Name", "Name");

            return View(model);
        }
        //POST: Admin/CreateUser/{user}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdminCreateUser(RegisterViewModel registerViewModel)
        {
            if (ModelState.IsValid)
            {
                var recent_Id = context.Users.Max(i => i.IId);

                var user = new ApplicationUser
                {
                    FirstName = registerViewModel.FirstName,
                    LastName = registerViewModel.LastName,
                    Birthday = registerViewModel.Birthday,
                    DateCreated = registerViewModel.DateCreated,
                    Email = registerViewModel.Email,
                    UserName = registerViewModel.UserName,
                    IId = recent_Id + 1
                };
                var passHash = UserManager.PasswordHasher.HashPassword(registerViewModel.Password);
                var result = await UserManager.CreateAsync(user, passHash);
                TempData["Success"] = "User Created Successfully";
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, registerViewModel.UserRoles);
                }
                return RedirectToAction("ViewAllUsers");
            }
            // If we got this far, something failed, redisplay form
            return View(registerViewModel);
        }

        // GET: /User/Edit/{id}

    }
}