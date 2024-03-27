using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FaceApp.Models
{
    public class ApplicationRole : IdentityRole<int>
    {
        public ApplicationRole() : base()
        { }
        public ApplicationRole(string roleName) : base(roleName)
        {
        }
        public string Description { get; set; }
        private DateTime? createdOn;
        [DataType(DataType.DateTime)]
        public DateTime CreatedOn
        {
            get { return createdOn ?? DateTime.UtcNow; }
            set { createdOn = value; }
        }
    }
}
