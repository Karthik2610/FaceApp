using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace FaceApp.Models.ViewModels
{
   
    public class ProgramViewModel
    {
        public int ProgramId { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("Program Name")]
        [Required(ErrorMessage = "Enter Program Name")]
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }

        public string ProgramID1 { get; set; }
        public string ProgramID2 { get; set; }
        public string ProgramID3 { get; set; }
        public string GLAccountCode { get; set; }
        public int? LoginUserId { get; set; }

        public int? CalendarId { get; set; }
        public List<SelectListItem> CalendarList { get; set; }
        public List<ProgramCalendarListViewModel> ProgramCalendarList { get; set; }
    }

    public class ProgramCalendarListViewModel
    {
        public int? ProgramCalendarDetailsId { get; set; }
        public int? ProgramId { get; set; }
        public int CalendarId { get; set; }
        public string CalendarName { get; set; }
    }

    public class ProgramListViewModel
    {
        public int LoginUserId { get; set; }
        public bool IsAdmin { get; set; }
        public string ProgramName { get; set; }
        public List<ProgramList> programList { get; set; }
    }

    public class ProgramList
    {
        public int ProgramId { get; set; }
        public string ProgramName { get; set; }
        public bool IsActive { get; set; }
        public int TotalCount { get; set; }
    }

}