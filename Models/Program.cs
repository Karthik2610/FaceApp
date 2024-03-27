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
    public class Programs
    {
        [Key]
        public int ProgramId { get; set; }
        public string ProgramName { get; set; }
        public bool IsActive { get; set; }
        public string ProgramDescription { get; set; }
        public string ProgramId1 { get; set; }
        public string ProgramId2 { get; set; }
        public string ProgramId3 { get; set; }
        public string GLAccountCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
    }

    public class ProgramCalendarDetails
    {
        [Key]
        public int ProgramCalendarDetailsId { get; set; }
        [ForeignKey("ProgramId")]
        public virtual Programs Programs { get; set; }
        public int ProgramId { get; set; }
        [ForeignKey("CalendarId")]
        public virtual CalendarSetting CalendarSetting { get; set; }
        public int CalendarId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
    }

}
