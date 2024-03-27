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
    public class Locations
    {
        [Key]
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public bool IsActive { get; set; }
        public string Address1 { get; set; }

        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string AccountingCode { get; set; }
        public string LocationId1 { get; set; }
        public string LocationId2 { get; set; }
        public string LocationId3 { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class LocationProgramDetails
    {
        [Key]
        public int LocationProgramDetailsId { get; set; }
        [ForeignKey("LocationId")]
        public virtual Locations Locations { get; set; }
        public int LocationId { get; set; }
        [ForeignKey("ProgramId")]
        public virtual Programs Programs { get; set; }
        public int ProgramId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
    }

}
