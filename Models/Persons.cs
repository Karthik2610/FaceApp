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
    public class Persons
    {
        [Key]
        public int PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleInitial { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public DateTime? DOB { get; set; }
        public string PersonId1 { get; set; }
        public string PersonId2 { get; set; }
        public string PersonId3 { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class PersonTrainDetails
    {
        [Key]
        public Guid PersonTrainId { get; set; }
        public int PersonId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? NoOfTrainings { get; set; }
    }

    public class PersonsDetails
    {
        [Key]
        public int PersonsDetailsId { get; set; }
        [ForeignKey("PersonId")]
        public virtual Persons Persons { get; set; }
        public int PersonId { get; set; }
        [ForeignKey("LocationId")]
        public virtual Locations Locations { get; set; }
        public int LocationId { get; set; }
        [ForeignKey("ProgramId")]
        public virtual Programs Programs { get; set; }
        public int ProgramId { get; set; }
        [ForeignKey("CalendarId")]
        public virtual CalendarSetting CalendarSetting { get; set; }
        public int CalendarId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
    }

}
