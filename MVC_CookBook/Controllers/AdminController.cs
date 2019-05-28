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
            ViewBag.UsersCount = context.Users.Count();

            ViewBag.RolesCount = context.Roles.Count();

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

        // GET: /Admin/AdminEditUser/{id}
        [HttpGet]
        public async Task<ActionResult> AdminEditUser(string guid)
        {
            if(guid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await UserManager.FindByIdAsync(guid);
            if(user == null)
            {
                return HttpNotFound();
            }
            var role = await UserManager.GetRolesAsync(user.Id);

            var theUser = new AdminUserViewModel
            {
                Guid = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                Birthday = user.Birthday,
                DateCreated = user.DateCreated,
                UserRole = role.FirstOrDefault(),
                Id = user.IId
            };

            return View(theUser);
        }
        // POST: /Admin/AdminEditUser/{model}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdminEditUser([Bind]AdminUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(model.Guid);
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Birthday = model.Birthday;
                user.DateCreated = model.DateCreated;
                user.UserName = model.UserName;
                user.IId = model.Id;

                var result = await UserManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await context.SaveChangesAsync();
                    TempData["Success"] = "User Updated Successfully";
                    return RedirectToAction("ViewAllUsers");
                }
                else
                {
                    TempData["Error"] = "User Update Unsuccessful";
                    return RedirectToAction("ViewAllUsers");
                }
            }
            else
            {
                TempData["Error"] = "User Update Unsuccessful";
                return RedirectToAction("ViewAllUsers");
            }
        }

        public async Task<ActionResult> AdminRoleManager()
        { // Create Role DropDownList
            GetRolesSelectList();
            GetUsersSelectList();
            GetUsersViewBag();

            return await Task.Run(() => View());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GetUserRole(string userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var user = await UserManager.FindByIdAsync(userId);
                var role = await UserManager.GetRolesAsync(user.Id);

                ViewBag.Roles4User = role;
                TempData["Success"] = "Roles Retrieved Successfully";

                GetRolesSelectList();
                GetUsersSelectList();
                GetUsersViewBag();

                return View("AdminRoleManager");
            }
            else
            {
                TempData["Error"] = "Role Retrieval Unsuccessful";
                return RedirectToAction("AdminRoleManager");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeUserRole(string userId, string role)
        {
            if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(role))
            {
                var user = await UserManager.FindByIdAsync(userId);
                var currentRole = await UserManager.GetRolesAsync(user.Id);
                await UserManager.RemoveFromRoleAsync(user.Id, currentRole.FirstOrDefault());
                await UserManager.AddToRoleAsync(user.Id, role);
                TempData["Success"] = "Role Successfully Changed.";

                GetRolesSelectList();
                GetUsersSelectList();
                GetUsersViewBag();

                return View("AdminRoleManager");
            }
            else
            {
                TempData["Error"] = "Role Change Unsuccessful!";
                return RedirectToAction("RoleManager");
            }
        }

        public void GetUsersViewBag()
        {
            var userNrole = (from user in context.Users
                             from role in user.Roles
                             join r in context.Roles
                             on role.RoleId
                             equals r.Id
                             orderby user.IId
                             select new AdminUserViewModel()
                             {
                                 Guid = user.Id,
                                 Id = user.IId,
                                 FirstName = user.FirstName,
                                 LastName = user.LastName,
                                 UserName = user.UserName,
                                 Email = user.Email,
                                 Birthday = user.Birthday,
                                 DateCreated = user.DateCreated,
                                 UserRole = r.Name
                             }).ToList();
            ViewBag.UsersPlusRoles = userNrole;
        }
        public void GetRolesSelectList()
        {
            var roleList = context.Roles.ToList()
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToList();

            ViewBag.Roles = roleList;
        }
        public void GetUsersSelectList()
        {
            var users = context.Users.ToList()
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.UserName
                }).ToList();

            ViewBag.UsersDDL = users;
        }
    }
}