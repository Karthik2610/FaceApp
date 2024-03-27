using FaceApp.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FaceApp.Models
{
    public class Face
    {
        [Key]
        public int ImageId { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        [DisplayName("Person Name")]
        public string PersonName { get; set; }

        [NotMapped]
        [DisplayName("Uploaded File")]
        public IFormFile UploadedFile { get; set; }

        [NotMapped]
        [DisplayName("Captured File")]
        public string CapturedFile { get; set; }

        [NotMapped]
        [DisplayName("Brower Name")]
        public string BrowserName { get; set; }

        //[Required(ErrorMessage = "Please Select Mode of transportation")]
        [DisplayName("Mode of transportation")]
        [SelectRequiredForAnyAttribute(Values = new[] { "CheckIn" }, PropertyName = nameof(AttendanceType),ErrorMessage = "Select Mode of transportation")]
        public string Modeoftransportation { get; set; }
        [StringLength(10, ErrorMessage= "TransitNumber cannot exceed 10 characters.")]
        public string TransitNumber { get; set; }
        [Required(ErrorMessage = "Please Select Attendance Type")]
        public string AttendanceType { get; set; }
        [Required(ErrorMessage = "Please Select Location")]
        public int? LocationId { get; set; }
        [Required(ErrorMessage = "Please Select Program")]
        public int? ProgramId { get; set; }
        public bool IsContinueWithCheckOut { get; set; }
        public DateTime? LastCheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public List<SelectListItem> ProgramList { get; set; }
        public List<SelectListItem> LocationList { get; set; }
    }
    public class PersonDetails
    {
        [Key]
        public Guid PersonId { get; set; }
        public string PersonName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class SelectRequiredForAnyAttribute : ValidationAttribute
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
                return new ValidationResult($" Select {Name}");
            }
            return ValidationResult.Success;
        }
    }

    //public class Person
    //{

    //    public int PersonID { get; set; }
    //}
}
