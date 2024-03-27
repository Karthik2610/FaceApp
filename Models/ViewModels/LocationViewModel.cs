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
   
    public class LocationViewModel
    {
        public int LocationId { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        [DisplayName("Location Name")]
        [Required(ErrorMessage = "Enter Location Name")]
        public string Name { get; set; }
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
        
        public int? ProgramId { get; set; }
        public int? LoginUserId { get; set; }

        public int? CalendarId { get; set; }
        public List<SelectListItem> ProgramList { get; set; }
        public List<LocationProgramListViewModel> LocationProgramList { get; set; }
    }

    public class LocationProgramListViewModel
    {
        public int? LocationProgramDetailsId { get; set; }
        public int? LocationId { get; set; }
        public int ProgramId { get; set; }
        public string ProgramName { get; set; }
    }

    public class LocationListViewModel
    {
        public int LoginUserId { get; set; }
        public bool IsAdmin { get; set; }
        public string LocationName { get; set; }
        public List<LocationList> LocationList { get; set; }
    }

    public class LocationList
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public bool IsActive { get; set; }
        public int TotalCount { get; set; }
    }

}