using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace FaceApp.Models.ViewModels
{
   
    public class UsersViewModel
    {
        public int UserId { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("User Employer ID")]
        //[Required(ErrorMessage = "Enter User Emp Id")]
        public String UserEmpId { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("First Name")]
        [Required(ErrorMessage = "Enter First Name")]
        public string FirstName { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("Last Name")]
        [Required(ErrorMessage = "Enter Last Name")]
        public string LastName { get; set; }


        [Column(TypeName = "nvarchar(50)")]
        [DisplayName("Middle Initial")]
        public string MiddleInitial { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("Email")]
        [Required(ErrorMessage = "Enter Email")]
        public string Email { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("Password")]
        [RequiredForAny(Values = new[] { "True" }, PropertyName = nameof(IsPasswordChanged))]
        public string Password { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        [DisplayName("Security Group")]
        public string SecurityGroup { get; set; }
        public bool IsPasswordChanged { get; set; }
        public int LoginUserId { get; set; }
        public bool IsActive { get; set; }
        public int? LocationId { get; set; }
        public List<SelectListItem> LocationsList { get; set; }
        public List<UserLocationsListViewModel> UserLocationsList { get; set; }

    }

    public class UserLocationsListViewModel
    {
        public int? UserLocationsDetailsId { get; set; }
        public int? UserId { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }
    }

    public class UsersListViewModel
    {
        public int LoginUserId { get; set; }
        public bool IsAdmin { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<UsersList> usersLists { get; set; }
    }

    public class UsersList
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmpId { get; set; }
        public string SecurityGroup { get; set; }
        public string Email { get; set; }
        public bool? IsAdmin { get; set; }
        public int TotalCount { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class RequiredForAnyAttribute : ValidationAttribute
    {
        /// <summary>
        /// Values of the <see cref="PropertyName"/> that will trigger the validation
        /// </summary>
        public string[] Values { get; set; }

        /// <summary>
        /// Independent property name
        /// </summary>
        public string PropertyName { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = validationContext.ObjectInstance;
            if (model == null || Values == null)
            {
                return ValidationResult.Success;
            }

            var currentValue = model.GetType().GetProperty(PropertyName)?.GetValue(model, null)?.ToString();
            if (Values.Contains(currentValue) && (value == null || value.ToString() == ""))
            {
                var propertyInfo = validationContext.ObjectType.GetProperty(validationContext.MemberName);
                var Name = validationContext.DisplayName != null ? validationContext.DisplayName : propertyInfo.Name;
                return new ValidationResult($" Enter  {Name}");
            }
            return ValidationResult.Success;
        }
    }
}