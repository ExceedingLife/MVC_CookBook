using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVC_CookBook.Models
{
    public class UserModels {    }

    public class UserViewModel
    {
        [Display(Name = "User ID")]
        public int Id { get; set; }

        public string Guid { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        public string Email { get; set; }

        public DateTime Birthday { get; set; }

        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }

    }

}