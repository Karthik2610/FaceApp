using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace FaceApp.Models.ViewModels
{
    public class LoginViewModel
    {

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("Email")]
        [Required(ErrorMessage = "Enter Email")]
        public string Email { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("Password")]
        [Required(ErrorMessage = "Enter Password")]
        public string Password { get; set; }
    }

    public class ForgotPasswordModel
    {

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("Email")]
        [Required(ErrorMessage = "Enter Email")]
        public string Email { get; set; }
        public int UserId { get; set; }
        public string Password { get; set; }

        public string Salt { get; set; }
        public int LoginUserId { get; set; }
    }

    public class ChangePasswordModel
    {

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("Old Password")]
        [Required(ErrorMessage = "Old Password")]
        public string OldPassword { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("New Password")]
        [Required(ErrorMessage = "Enter New Password")]
        public string NewPassword { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("Confirm New Password")]
        [Required(ErrorMessage = "Enter Confirm Password")]
        public string Password { get; set; }
        public int UserId { get; set; }

        public bool isLoginRedirect { get; set; }
        public string Salt { get; set; }
        public int LoginUserId { get; set; }
    }
}