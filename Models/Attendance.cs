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
    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }
        public int PersonId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string ModeofTransportation { get; set; }
        [StringLength(10, ErrorMessage = "The {0} value cannot exceed {10} characters. ")]
        public string TransitNumber { get; set; }
        public string AttendanceType { get; set; }
        [ForeignKey("LocationId")]
        public virtual Locations Locations { get; set; }
        public int? LocationId { get; set; }
        [ForeignKey("ProgramId")]
        public virtual Programs Programs { get; set; }
        public int? ProgramId { get; set; }
        public int CreatedBy { get; set; }
    }
    
}
