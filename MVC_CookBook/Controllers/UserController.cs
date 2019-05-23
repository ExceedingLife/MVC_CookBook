using System;
using System.Net;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using MVC_CookBook.Models;

namespace MVC_CookBook.Controllers
{
    public class UserController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext context;

        public UserController()
        {
            context = new ApplicationDbContext();
        }
        public UserController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            context = new ApplicationDbContext();
        }
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext
                    .GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set { _signInManager = value; }
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext
                    .GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set { _userManager = value; }
        }
        public IAuthenticationManager AuthManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        // GET: User/GetAllUsers/
        [HttpGet]
        public ActionResult GetAllUsers()
        {
            var userDetails = (from user in context.Users
                               from userrole in user.Roles
                               join role in context.Roles
                               on userrole.RoleId
                               equals role.Id
                               select new UserViewModel()
                               {
                                   Id = user.IId,
                                   Guid = user.Id,
                                   FirstName = user.FirstName,
                                   LastName = user.LastName,
                                   Birthday = user.Birthday,
                                   DateCreated = user.DateCreated,
                                   Email = user.Email,
                                   UserName = user.UserName,
                                   UserRole = role.Name
                               }).ToList();
            if(userDetails == null)
            {
                return HttpNotFound();
            }
            return View(userDetails);
        }


    }
}