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
   
    public class PersonsViewModel
    {
        public int PersonId { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("First Name")]
        [Required(ErrorMessage = "Enter First Name")]
        public string FirstName { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("Last Name")]
        [Required(ErrorMessage = "Enter Last Name")]
        public string LastName { get; set; }


        [Column(TypeName = "nvarchar(50)")]
        [DisplayName("Middle Initial")]
        public string MiddleInitial { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("Address 1")]
        public string Address1 { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("Address 2")]
        public string Address2 { get; set; }


        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("City")]
        public string City { get; set; }


        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("State")]
        public string State { get; set; }


        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("Zip Code")]
        public string Zip { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("DOB")]
        public string DOB { get; set; }
        [DisplayName("Person ID 1")]
        public string PersonId1 { get; set; }
        [DisplayName("Person ID 2")]
        public string PersonId2 { get; set; }
        [DisplayName("Person ID 3")]
        public string PersonId3 { get; set; }
        [DisplayName("Is Active")]
        public bool IsActive { get; set; }

        public List<ServiceAssignmentsViewModel> ServiceAssignments { get; set; }

        public int ImageId { get; set; }

        [NotMapped]
        [DisplayName("Uploaded File")]
        public IFormFile UploadedFile { get; set; }

        [NotMapped]
        [DisplayName("Captured File")]
        public string CapturedFile { get; set; }

        [NotMapped]
        [DisplayName("Brower Name")]
        public string BrowserName { get; set; }

        public int LoginUserId { get; set; }
        public bool IsClearTrain { get; set; }
        public int? LocationId { get; set; }
        public int? ProgramId { get; set; }
        public int? CalendarId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<SelectListItem> LocationsList { get; set; }
        public List<SelectListItem> ProgramsList { get; set; }
        public List<SelectListItem> CalendarsList { get; set; }
        public List<PersonDetailsListViewModel> PersonDetailsList{ get; set; }

        [Display(Name = "Last trained on")]
        public DateTime? LastTrainedOn { get; set; }
        [Display(Name = "No of times trained")]
        public int? NoOfTrainings { get; set; }

    }

    public class PersonDetailsListViewModel
    {
        public int? PersonsDetailsId { get; set; }
        public int? PersonId { get; set; }
        public int? LocationId { get; set; }
        public string LocationName { get; set; }
        public int? ProgramId { get; set; }
        public string ProgramName { get; set; }
        public int? CalendarId { get; set; }
        public string CalendarName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
    public class ServiceAssignmentsViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsActive { get; set; }
    }

    public class PersonsListViewModel
    {
        public int LoginUserId { get; set; }
        public bool IsAdmin { get; set; }
        public string PersonName { get; set; }
        //public string LastName { get; set; }
        public List<PersonsList> personsList { get; set; }
    }

    public class CalendarDuplicateCheckModel
    {
        public int? RowId { get; set; }
        public int? CalendarId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? RowCalendarId { get; set; }
        public DateTime? RowStartDate { get; set; }
        public DateTime? RowEndDate { get; set; }
    }

    public class PersonsList
    {
        public int PersonId { get; set; }
        public string Name { get; set; }
        public string DOB { get; set; }
        public string PersonId1 { get; set; }
        public bool IsActive { get; set; }
        public int TotalCount { get; set; }
    }

    public class AttendanceList
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AttendanceDate { get; set; }
        public string ModeofTransportation { get; set; }
        public string CheckInTime { get; set; }
        public string CheckOutTime { get; set; }
        public string Duration { get; set; }
        public int? TotalCount { get; set; }

        public string PersonName { get; set; }//Field
        public bool? CheckIn { get; set; }//Field
        public bool? CheckOut { get; set; }//Field
        public int? Col1 { get; set; }//Field
        public int? Col2 { get; set; }//Field
        public int? Col3 { get; set; }//Field
        public int? Col4 { get; set; }//Field
        public int? Col5 { get; set; }//Field
        public int? Col6 { get; set; }//Field
        public int? Col7 { get; set; }//Field
        public int? Col8 { get; set; }//Field
        public int? Col9 { get; set; }//Field
        public int? Col10 { get; set; }//Field
        public int? Col11 { get; set; }//Field
        public int? Col12 { get; set; }//Field
        public int? Col13 { get; set; }//Field
        public int? Col14 { get; set; }//Field
        public int? Col15 { get; set; }//Field
        public int? Col16 { get; set; }//Field
        public int? Col17 { get; set; }//Field
        public int? Col18 { get; set; }//Field
        public int? Col19 { get; set; }//Field
        public int? Col20 { get; set; }//Field
        public int? Col21 { get; set; }//Field
        public int? Col22 { get; set; }//Field
        public int? Col23 { get; set; }//Field
        public int? Col24 { get; set; }//Field
        public int? Col25 { get; set; }//Field
        public int? Col26 { get; set; }//Field
        public int? Col27 { get; set; }//Field
        public int? Col28 { get; set; }//Field
        public int? Col29 { get; set; }//Field
        public int? Col30 { get; set; }//Field
        public int? Col31 { get; set; }//Field
        public int? Col32 { get; set; }//Field
        public int? Col33 { get; set; }//Field
        public int? Col34 { get; set; }//Field
        public int? Col35 { get; set; }//Field
        public int? Col36 { get; set; }//Field
        public int? Col37 { get; set; }//Field
        public int? Col38 { get; set; }//Field
        public int? Col39 { get; set; }//Field
        public int? Col40 { get; set; }//Field
        public int? Col41 { get; set; }//Field
        public int? Col42 { get; set; }//Field
        public int? Col43 { get; set; }//Field
        public int? Col44 { get; set; }//Field
        public int? Col45 { get; set; }//Field
        public int? Col46 { get; set; }//Field
        public int? Col47 { get; set; }//Field
        public int? Col48 { get; set; }//Field
        public int? Col49 { get; set; }//Field
        public int? Col50 { get; set; }//Field
        public int? Col51 { get; set; }//Field
        public int? Col52 { get; set; }//Field
        public int? Col53 { get; set; }//Field
        public int? Col54 { get; set; }//Field
        public int? Col55 { get; set; }//Field
        public int? Col56 { get; set; }//Field
        public int? Col57 { get; set; }//Field
        public int? Col58 { get; set; }//Field
        public int? Col59 { get; set; }//Field
        public int? Col60 { get; set; }//Field
        public int? Col61 { get; set; }//Field
        public int? Col62 { get; set; }//Field
        public int? Col63 { get; set; }//Field
        public int? Col64 { get; set; }//Field
        public int? Col65 { get; set; }//Field
        public int? Col66 { get; set; }//Field
        public int? Col67 { get; set; }//Field
        public int? Col68 { get; set; }//Field
        public int? Col69 { get; set; }//Field
        public int? Col70 { get; set; }//Field
        public int? Col71 { get; set; }//Field
        public int? Col72 { get; set; }//Field
        public int? Col73 { get; set; }//Field
        public int? Col74 { get; set; }//Field
        public int? Col75 { get; set; }//Field
        public int? Col76 { get; set; }//Field
        public int? Col77 { get; set; }//Field
        public int? Col78 { get; set; }//Field
        public int? Col79 { get; set; }//Field
        public int? Col80 { get; set; }//Field
        public int? Col81 { get; set; }//Field
        public int? Col82 { get; set; }//Field
        public int? Col83 { get; set; }//Field
        public int? Col84 { get; set; }//Field
        public int? Col85 { get; set; }//Field
        public int? Col86 { get; set; }//Field
        public int? Col87 { get; set; }//Field
        public int? Col88 { get; set; }//Field
        public int? Col89 { get; set; }//Field
        public int? Col90 { get; set; }//Field
        public int? Col91 { get; set; }//Field
        public int? Col92 { get; set; }//Field
        public int? Col93 { get; set; }//Field
        public int? Col94 { get; set; }//Field
        public int? Col95 { get; set; }//Field
        public int? Col96 { get; set; }//Field
        public int? Col97 { get; set; }//Field
        public int? Col98 { get; set; }//Field
        public int? Col99 { get; set; }//Field
        public int? Col100 { get; set; }//Field
        public int? Col101 { get; set; }//Field
        public int? Col102 { get; set; }//Field
        public int? Col103 { get; set; }//Field
        public int? Col104 { get; set; }//Field
        public int? Col105 { get; set; }//Field
        public int? Col106 { get; set; }//Field
        public int? Col107 { get; set; }//Field
        public int? Col108 { get; set; }//Field
        public int? Col109 { get; set; }//Field
        public int? Col110 { get; set; }//Field
        public int? Col111 { get; set; }//Field
        public int? Col112 { get; set; }//Field
        public int? Col113 { get; set; }//Field
        public int? Col114 { get; set; }//Field
        public int? Col115 { get; set; }//Field
        public int? Col116 { get; set; }//Field
        public int? Col117 { get; set; }//Field
        public int? Col118 { get; set; }//Field
        public int? Col119 { get; set; }//Field
        public int? Col120 { get; set; }//Field
        public int? Col121 { get; set; }//Field
        public int? Col122 { get; set; }//Field
        public int? Col123 { get; set; }//Field
        public int? Col124 { get; set; }//Field
        public int? Col125 { get; set; }//Field
        public string LocationName { get; set; }
        public string PersonID1 { get; set; }
        public string ProgramName { get; set; }
        public string CalendarName { get; set; }
    }
    public class AttendanceListViewModel
    {
        [Required(ErrorMessage = "Select Report Date")]
        public string ReportDate { get; set; }
        public string ReportType { get; set; }
        public int? LocationId { get; set; }
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }
        public int? PageSize { get; set; }
        public int? PageIndex { get; set; }
        public List<AttendanceList> AttendanceList { get; set; }
        public List<SelectListItem> ReportTypeList { get; set; }
        public List<SelectListItem> LocationsList { get; set; }
    }
    
    public class AttendanceGridFieldsListViewModel
    {
        public string name { get; set; }//Field
        public string title { get; set; }//Field
    }
}