using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace FaceApp.Models.ViewModels
{
   
    public class SettingsViewModel
    {
        public int CalendarId { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("Calendar Name")]
        [Required(ErrorMessage = "Enter Calendar Name")]
        public string Name { get; set; }
        public bool IsActive { get; set; }
        [DisplayName("Sunday Start Time")]
        [RequiredForAny(Values = new[] { "True" }, PropertyName = nameof(SundayIsActive))]
        public string SundayStartTime { get; set; }
        [DisplayName("Sunday End Time")]
        [RequiredForAny(Values = new[] { "True" }, PropertyName = nameof(SundayIsActive))]
        public string SundayEndTime { get; set; }
        public bool SundayIsActive { get; set; }
        [DisplayName("Monday Start Time")]
        [RequiredForAny(Values = new[] { "True" }, PropertyName = nameof(MondayIsActive))]
        public string MondayStartTime { get; set; }
        [DisplayName("Monday End Time")]
        [RequiredForAny(Values = new[] { "True" }, PropertyName = nameof(MondayIsActive))]
        public string MondayEndTime { get; set; }
        public bool MondayIsActive { get; set; }
        [DisplayName("Tuesday Start Time")]
        [RequiredForAny(Values = new[] { "True" }, PropertyName = nameof(TuesdayIsActive))]
        public string TuesdayStartTime { get; set; }
        [DisplayName("Tuesday End Time")]
        [RequiredForAny(Values = new[] { "True" }, PropertyName = nameof(TuesdayIsActive))]
        public string TuesdayEndTime { get; set; }
        public bool TuesdayIsActive { get; set; }
        [DisplayName("Wednesday Start Time")]
        [RequiredForAny(Values = new[] { "True" }, PropertyName = nameof(WednesdayIsActive))]
        public string WednesdayStartTime { get; set; }
        [DisplayName("Wednesday End Time")]
        [RequiredForAny(Values = new[] { "True" }, PropertyName = nameof(WednesdayIsActive))]
        public string WednesdayEndTime { get; set; }
        public bool WednesdayIsActive { get; set; }
        [DisplayName("Thursday Start Time")]
        [RequiredForAny(Values = new[] { "True" }, PropertyName = nameof(ThursdayIsActive))]
        public string ThursdayStartTime { get; set; }
        [DisplayName("Thursday End Time")]
        [RequiredForAny(Values = new[] { "True" }, PropertyName = nameof(ThursdayIsActive))]
        public string ThursdayEndTime { get; set; }
        public bool ThursdayIsActive { get; set; }
        [DisplayName("Friday Start Time")]
        [RequiredForAny(Values = new[] { "True" }, PropertyName = nameof(FridayIsActive))]
        public string FridayStartTime { get; set; }
        [DisplayName("Friday End Time")]
        [RequiredForAny(Values = new[] { "True" }, PropertyName = nameof(FridayIsActive))]
        public string FridayEndTime { get; set; }
        public bool FridayIsActive { get; set; }
        [RequiredForAny(Values = new[] { "True" }, PropertyName = nameof(SaturdayIsActive))]
        [DisplayName("Saturday Start Time")]
        public string SaturdayStartTime { get; set; }
        [DisplayName("Saturday End Time")]
        [RequiredForAny(Values = new[] { "True" }, PropertyName = nameof(SaturdayIsActive))]
        public string SaturdayEndTime { get; set; }
        public bool SaturdayIsActive { get; set; }
        public int LoginUserId { get; set; }
    }

    //public class ServiceAssignmentsViewModel
    //{
    //    public int ServiceId { get; set; }
    //    public string ServiceName { get; set; }
    //    public DateTime? StartDate { get; set; }
    //    public DateTime? EndDate { get; set; }
    //    public bool? IsActive { get; set; }
    //}

    public class SettingsListViewModel
    {
        public int LoginUserId { get; set; }
        public bool IsAdmin { get; set; }
        public string CalendarName { get; set; }
        public List<SettingsList> settingsList { get; set; }
    }

    public class SettingsList
    {
        public int CalendarId { get; set; }
        public string CalendarName { get; set; }
        public bool IsActive { get; set; }
        public int TotalCount { get; set; }
    }

}