using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FaceApp.Models
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }

        public string UserEmpId { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("Last Name")]
        public string LastName { get; set; }


        [Column(TypeName = "nvarchar(50)")]
        [DisplayName("Middle Initial")]
        public string MiddleInitial{ get; set; }

        [Column(TypeName = "nvarchar(50)")]
        [DisplayName("Email")]
        public string Email { get; set; }

        [DisplayName("Password")]
        public string Password { get; set; }

        [DisplayName("Salt")]
        public string Salt { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsActive { get; set; }

        public bool IsForgotPassword { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        [DisplayName("Security Group")]
        public string SecurityGroup { get; set; }

        public DateTime CreatedDate { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }

    }
    public class UserLocationDetails
    {
        [Key]
        public int UserLocationDetailsId { get; set; }
        [ForeignKey("UserId")]
        public virtual Users Users { get; set; }
        public int UserId { get; set; }
        [ForeignKey("LocationId")]
        public virtual Locations Locations{ get; set; }
        public int LocationId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
    }

}
