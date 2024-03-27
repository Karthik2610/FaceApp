using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace FaceApp.Models.ViewModels
{
   
    public class ReportsViewModel
    {
        public string ReportType { get; set; }
        public int? ProgramId { get; set; }
        public int? LocationId { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }
        public int? PageSize { get; set; }
        public int? PageIndex { get; set; }
   
        public List<SelectListItem> ReportTypeList { get; set; }
        public List<SelectListItem> LocationsList { get; set; }

        public List<SelectListItem> ProgramsList { get; set; }

    }

    public class ReportListViewModel
    {
        public string Name { get; set; }
        public bool? StatusIn { get; set; }
        public int? TotalCount { get; set; }
        public int? CheckOutCount { get; set; }
        public int? CheckInCount { get; set; }
        public int? ScheduledCount { get; set; }
        public bool? StatusOut { get; set; }
        public string ProgramName { get; set; }
        public string LocationName { get; set; }
        public string Date { get; set; }
        public string Duration { get; set; }        
        public string PersonID1 { get; set; }        
    }

}