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
    public class CalendarSetting
    {
        [Key]
        public int CalendarId { get; set; }
        public string CalendarName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? SundayStartTime { get; set; }
        public DateTime? SundayEndTime { get; set; }
        public bool SundayIsActive { get; set; }
        public DateTime? MondayStartTime { get; set; }
        public DateTime? MondayEndTime { get; set; }
        public bool MondayIsActive { get; set; }
        public DateTime? TuesdayStartTime { get; set; }
        public DateTime? TuesdayEndTime { get; set; }
        public bool TuesdayIsActive { get; set; }
        public DateTime? WednesdayStartTime { get; set; }
        public DateTime? WednesdayEndTime { get; set; }
        public bool WednesdayIsActive { get; set; }
        public DateTime? ThursdayStartTime { get; set; }
        public DateTime? ThursdayEndTime { get; set; }
        public bool ThursdayIsActive { get; set; }
        public DateTime? FridayStartTime { get; set; }
        public DateTime? FridayEndTime { get; set; }
        public bool FridayIsActive { get; set; }
        public DateTime? SaturdayStartTime { get; set; }
        public DateTime? SaturdayEndTime { get; set; }
        public bool SaturdayIsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
    }

}
