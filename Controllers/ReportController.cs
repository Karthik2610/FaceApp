using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FaceApp.Models;
using FaceApp.Services;
using FaceApp.Models.ViewModels;
using FaceApp.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using ClosedXML.Excel;
using System.IO;
using System.Collections;

namespace FaceApp.Controllers
{
    public class ReportController : Controller
    {

        private readonly FaceAppDBContext _context;
        private readonly ILogger<PersonController> _logger;
        private PersonService personService = null;
        private ReportService reportService = null;
        public ReportController(FaceAppDBContext context, ILogger<PersonController> logger)
        {
            _context = context;
            personService = new PersonService(context);
            reportService = new ReportService(context);
            _logger = logger;
        }
        public IActionResult AttendanceReport()
        {
            AttendanceListViewModel model = new AttendanceListViewModel();
            model.AttendanceList = new List<AttendanceList>();
            model.ReportTypeList = new List<SelectListItem>();
            model.LocationsList = new List<SelectListItem>();
            LoadReportsDropDownList(model);
            return View(model);
        }

        public IActionResult DailyWeeklyReport()
        {
            ReportsViewModel model = new ReportsViewModel();
            model.ReportTypeList = new List<SelectListItem>();
            model.ProgramsList = new List<SelectListItem>();
            model.LocationsList = new List<SelectListItem>();
            LoadDailyWeeklyReportDropDownList(model);
            return View(model);
        }

        /*[HttpPost]
        public IActionResult AttendanceReport(AttendanceListViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.AttendanceList = personService.GetAttendanceList(model.ReportDate);
            }
            else
            {
                ViewBag.Error = "Please select the report date";
                model.AttendanceList = new List<AttendanceList>();
            }
             return View(model);
        }*/

        private void LoadDailyWeeklyReportDropDownList(ReportsViewModel model)
        {
            //Load Report Type Options
            model.ReportTypeList.Add(new SelectListItem { Value = "Daily Report", Text = "Daily Report" });
            model.ReportTypeList.Add(new SelectListItem { Value = "Weekly Report", Text = "Weekly Report" });
            //Load Report Type Options

            model.LocationsList = personService.GetLocationsList(null);
            model.ProgramsList = personService.GetProgramsList(null);
        }

        private void LoadReportsDropDownList(AttendanceListViewModel model)
        {
            //Load Report Type Options
            model.ReportTypeList.Add(new SelectListItem { Value = "Attendance Report", Text = "Attendance Report" });
            model.ReportTypeList.Add(new SelectListItem { Value = "Attendance Detail Report", Text = "Attendance Detail Report" });
            //Load Report Type Options

            model.LocationsList = personService.GetLocationsList(null);
        }


        [HttpPost]
        public ActionResult GetAttendanceReportData(AttendanceListViewModel model, string pagestart = null, string pageend = null, string sortColumn = null, string sortOrder = null)
        {
            int pageStart = pagestart != null ? Convert.ToInt32(pagestart) : 0;
            int pageEnd = pageend != null ? Convert.ToInt32(pageend) : 0;
            int recordsTotal = 0;
            int sortorder = 0;
            if (sortOrder == "desc")
            {
                sortorder = 1;
            }

            if (string.IsNullOrEmpty(sortColumn))
            {
                sortColumn = "Name";
            }

            using (FaceAppDBContext dc = new FaceAppDBContext())
            {
                var reportsLists = new List<AttendanceList>();

                reportsLists = personService.GetAttendanceList(model, pageStart, pageEnd, sortColumn, sortorder);

                //ExportExcel(dt);
                //if (model.ReportType !=null && !model.ReportType.Contains("Detail"))
                //{
                recordsTotal = reportsLists.Select(s => s.TotalCount.Value).FirstOrDefault();
                //}

                var response = reportsLists.ToList();
                return Json(new
                {
                    response = response,
                    pageEnd = pageEnd,
                    pageStart = pageStart,
                    TotalRecords = recordsTotal,
                }, new Newtonsoft.Json.JsonSerializerSettings());
            }

        }

        //  [HttpGet("ExportToExcel")]
        public ActionResult ExportToExcelAR(AttendanceListViewModel model)
        {
            var errorMessage = "";
            var reportType = model.ReportType;
            if (model.ReportType == "Attendance Report")
            {
                if (string.IsNullOrEmpty(model.ReportType))
                {
                    errorMessage = "Report Type is missing";
                }
                if (string.IsNullOrEmpty(model.ReportDate))
                {
                    errorMessage = "Report Date is missing";
                }
               
            }
            else if (model.ReportType == "Attendance Detail Report")
            {
                if (string.IsNullOrEmpty(model.ReportType))
                {
                    errorMessage = "Report Type is missing";
                }
                if (string.IsNullOrEmpty(model.ReportDate))
                {
                    errorMessage = "Report Date is missing";
                }
                if (!model.LocationId.HasValue)
                {
                    errorMessage = "LocationID is missing";
                }
            }
            return Json(new { reportType, errorMessage });
        }
        public ActionResult ExportToExcelDWR(ReportsViewModel model)
        {
            var errorMessage = "";
            var reportType = model.ReportType;
            if (model.ReportType == "Daily Report")
            {
                if (string.IsNullOrEmpty(model.ReportType))
                {
                    errorMessage = "Report Type is missing";
                }
                if (!model.Date.HasValue)
                {
                    errorMessage = "ReportDatemissing";
                }
                if (!model.LocationId.HasValue)
                {
                    errorMessage = "LocationIdmissing";
                }
                if (!model.ProgramId.HasValue)
                {
                    errorMessage = "ProgramIdmissing";
                }
            }
            else if (model.ReportType == "Weekly Report")
            {
                if (string.IsNullOrEmpty(model.ReportType))
                {
                    errorMessage = "Report Type is missing";
                }
                else if (!model.FromDate.HasValue)
                {
                    errorMessage = "FromDatemissing";
                }
                else if (!model.ToDate.HasValue)
                {
                    errorMessage = "ToDatemissing";
                }
                else if (!model.LocationId.HasValue)
                {
                    errorMessage = "LocationIdmissing";
                }
                else if (!model.ProgramId.HasValue)
                {
                    errorMessage = "ProgramIdmissing";
                }
            }
            return Json(new { reportType, errorMessage });
        }

        [HttpGet]
        public ActionResult AttendanceDownload(string ReportDate, string LocationId, string ReportType)

        {
            AttendanceListViewModel model = new AttendanceListViewModel();
            model.ReportType = ReportType;
            model.ReportDate = ReportDate;
            model.LocationId = Convert.ToInt32(LocationId);
            int pageStart = 1;
            int pageEnd = 100000;
            int sortorder = 0;
            string sortColumn = "PersonName";
            DataTable dt = new DataTable();            
            if (model.ReportType == "Attendance Detail Report")
            {
                DataTable dtGetData = personService.GetAttendanceListDT(model, pageStart, pageEnd, sortColumn, sortorder);
                List<AttendanceGridFieldsListViewModel> programsList = reportService.GetProgramRowCount(ReportDate, model.LocationId);
                dtGetData.Columns.Remove("TotalCount");
                dtGetData.Columns.Remove("RowNumber");
                dtGetData.Columns.Remove("PersonId");

                dtGetData.Columns["PersonName"].ColumnName = "Person Name";                
                dtGetData.Columns["PersonID1"].ColumnName = "Person Id 1";                
                ArrayList al = new ArrayList();
                al.Add("Person Name");
                al.Add("Person Id 1");
                foreach (AttendanceGridFieldsListViewModel modelData in programsList)
                {
                    if (modelData.name.StartsWith("Col"))
                    {
                        string colName = modelData.name.Remove(0, 3);
                        dtGetData.Columns[colName].ColumnName = modelData.title;
                        al.Add(modelData.title);
                    }
                }
                ArrayList alRemoveList = new ArrayList();
                foreach (DataColumn column in dtGetData.Columns)
                {
                    if (!al.Contains(column.ColumnName))
                    {
                        alRemoveList.Add(column.ColumnName);
                    }

                }
                foreach (string colmnName in alRemoveList)
                {
                    dtGetData.Columns.Remove(colmnName);
                }
                dt = dtGetData.Clone();
                foreach (DataColumn column in dt.Columns)
                {
                    column.DataType = typeof(string);
                }
               
                foreach (DataRow dr in dtGetData.Rows)
                {
                    DataRow NewDr = dt.NewRow();
                    foreach (DataColumn column in dtGetData.Columns)
                    {
                        if (!string.IsNullOrEmpty(dr[column.ColumnName].ToString()))
                        {
                            if (column.ColumnName == "Person Name")
                            {                               
                                NewDr[column.ColumnName] = dr[column.ColumnName].ToString();
                                
                            }
                            else if (column.ColumnName == "Person Id 1")
                            {
                                NewDr[column.ColumnName] = dr[column.ColumnName].ToString();

                            }
                            else
                            {

                                if (dr[column.ColumnName].ToString() == "3")
                                {
                                    NewDr[column.ColumnName] = "Not Checked-In";
                                }
                                else if (dr[column.ColumnName].ToString() == "2")
                                {
                                    NewDr[column.ColumnName] = "Checked-In & Checked-Out Completed";
                                }
                                else if (dr[column.ColumnName].ToString() == "1")
                                {
                                    NewDr[column.ColumnName] = "Checked-In but not Checked-Out";
                                }
                                else
                                {
                                    NewDr[column.ColumnName] = string.Empty;
                                }
                            }

                        }
                    }
                    dt.Rows.Add(NewDr);
                }
            }
            else
            {
                dt = personService.GetAttendanceListDT(model, pageStart, pageEnd, sortColumn, sortorder);
                dt.Columns.Remove("TotalCount");
                dt.Columns.Remove("RowNumber");
                dt.Columns["FirstName"].ColumnName = "First Name";
                dt.Columns["LastName"].ColumnName = "Last Name";
                dt.Columns["AttendanceDate"].ColumnName = "Attendance Date";
                dt.Columns["ModeofTransportation"].ColumnName = "Mode of Transportation";
                dt.Columns["CheckInTime"].ColumnName = "Time in";
                dt.Columns["CheckOutTime"].ColumnName = "Time out";
                dt.Columns["Duration"].ColumnName = "Duration";
                dt.Columns["ProgramName"].ColumnName = "Program Name";
                dt.Columns["LocationName"].ColumnName = "Location Name";
                dt.Columns["PersonID1"].ColumnName = "Person Id 1";
            }
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add(dt);
                ws.Tables.FirstOrDefault().ShowAutoFilter = false;
                ws.Columns().AdjustToContents();
                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    return File(MyMemoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ReportType.Replace(" ","")+".xlsx");
                }
            }
        }
        [HttpGet]
        public ActionResult DailyDownload(string Date, string LocationId, string ReportType, string ProgramID)

        {
            ReportsViewModel model = new ReportsViewModel();
            model.ReportType = ReportType;
            model.Date = Convert.ToDateTime(Date);
            model.LocationId = Convert.ToInt32(LocationId);
            model.ProgramId = Convert.ToInt32(ProgramID);
            int pageStart = 1;
            int pageEnd = 100000;
            int sortorder = 0;
            string sortColumn = "Name";
            DataTable details = new DataTable();
            details.TableName = "Details";
            DataTable dt = reportService.GetDailyReportDataDT(model, pageStart, pageEnd, sortColumn, sortorder);
            if (ReportType == "Daily Report")
            {

                DataColumn dtColumn;
                DataRow myDataRow;
                // Create id column
                dtColumn = new DataColumn();
                dtColumn.DataType = typeof(Int32);
                dtColumn.ColumnName = "Scheduled";
                details.Columns.Add(dtColumn);
                dtColumn = new DataColumn();
                dtColumn.DataType = typeof(Int32);
                dtColumn.ColumnName = "Checked In";
                details.Columns.Add(dtColumn);
                dtColumn = new DataColumn();
                dtColumn.DataType = typeof(Int32);
                dtColumn.ColumnName = "Checked Out";
                details.Columns.Add(dtColumn);
                dtColumn = new DataColumn();
                dtColumn.DataType = typeof(Int32);
                dtColumn.ColumnName = "Missing";
                details.Columns.Add(dtColumn);

                int TotalCount = dt.AsEnumerable().Select(dr => dr.Field<int>("TotalCount")).FirstOrDefault();
                int checkInCount = dt.AsEnumerable().Select(dr => dr.Field<int>("CheckInCount")).FirstOrDefault();
                int checkOutCount = dt.AsEnumerable().Select(dr => dr.Field<int>("CheckOutCount")).FirstOrDefault();

                myDataRow = details.NewRow();
                myDataRow["Scheduled"] = TotalCount;
                myDataRow["Checked In"] = checkInCount;
                myDataRow["Checked Out"] = checkOutCount;
                myDataRow["Missing"] = TotalCount - checkInCount;
                details.Rows.Add(myDataRow);


                dt.Columns.Remove("TotalCount");
                dt.Columns.Remove("RowNumber");
                dt.Columns.Remove("CheckOutCount");
                dt.Columns.Remove("CheckInCount");
                dt.Columns.Remove("StatusIn");
                dt.Columns.Remove("StatusOut");
                dt.Columns["Name"].ColumnName = "Person Name";
                dt.Columns["StatusInExcel"].ColumnName = "Checked In";
                dt.Columns["StatusOutExcel"].ColumnName = "Checked Out";
                dt.Columns["ProgramName"].ColumnName = "Program Name";
                dt.Columns["LocationName"].ColumnName = "Location Name";
                dt.Columns["PersonId1"].ColumnName = "Person Id 1";
            }
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add(details);
                ws.Tables.FirstOrDefault().ShowAutoFilter = false;
                //wb.Worksheets.Add(details);
                ws.Cell(4, 1).InsertTable(dt).ShowAutoFilter = false;                
                
                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    return File(MyMemoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DailyReport.xlsx");
                }
            }
        }
        public ActionResult WeeklyDownload(string FromDate, string ToDate, string LocationId, string ReportType, string ProgramID)

        {
            ReportsViewModel model = new ReportsViewModel();
            model.ReportType = ReportType;
            model.FromDate = Convert.ToDateTime(FromDate);
            model.ToDate = Convert.ToDateTime(ToDate);
            model.LocationId = Convert.ToInt32(LocationId);
            model.ProgramId = Convert.ToInt32(ProgramID);
            int pageStart = 1;
            int pageEnd = 100000;
            int sortorder = 0;
            string sortColumn = "Name";
            DataTable dt = reportService.GetWeeklyReportDataDT(model, pageStart, pageEnd, sortColumn, sortorder);
            if (ReportType == "Weekly Report")
            {
                dt.Columns.Remove("TotalCount");
                dt.Columns.Remove("RowNumber");
                dt.Columns["Name"].ColumnName = "Person Name";
               // dt.Columns["ProgramName"].ColumnName = "Program Name";
                //dt.Columns["LocationName"].ColumnName = "Location Name";
                dt.Columns["AttendanceDate"].ColumnName = "Date";
                dt.Columns["Duration"].ColumnName = "Duration In (Hrs)";
                dt.Columns["PersonId1"].ColumnName = "Person Id 1";
            }
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add(dt);
                ws.Tables.FirstOrDefault().ShowAutoFilter = false;
                ws.Columns().AdjustToContents();
                
                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    return File(MyMemoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "WeeklyReport.xlsx");
                }
            }
        }
      

        [HttpPost]
        public ActionResult GenerateAttendanceDetailGridFields(string ReportDate, int? LocationId)
        {
            var reportsLists = new List<AttendanceListViewModel>();

            List<AttendanceGridFieldsListViewModel> programsList = reportService.GetProgramRowCount(ReportDate, LocationId);

            return Json(new { FieldList = programsList.ToList() }, new Newtonsoft.Json.JsonSerializerSettings());
        }

        [HttpPost]
        public ActionResult GetDailyWeeklyReportData(ReportsViewModel model, string pagestart = null, string pageend = null, string sortColumn = null, string sortOrder = null)
        {
            int pageStart = pagestart != null ? Convert.ToInt32(pagestart) : 0;
            int pageEnd = pageend != null ? Convert.ToInt32(pageend) : 0;
            int recordsTotal = 0;
            int checkInCount = 0;
            int checkOutCount = 0;
            int ScheduledCount = 0;
            int sortorder = 0;
            if (sortOrder == "desc")
            {
                sortorder = 1;
            }

            if (string.IsNullOrEmpty(sortColumn))
            {
                sortColumn = "Name";
            }

            using (FaceAppDBContext dc = new FaceAppDBContext())
            {
                var reportsLists = new List<ReportListViewModel>();
                if (model.ReportType == "Daily Report")
                {
                    reportsLists = reportService.GetDailyReportData(model, pageStart, pageEnd, sortColumn, sortorder);
                    checkInCount = reportsLists.Select(s => s.CheckInCount.Value).FirstOrDefault();
                    checkOutCount = reportsLists.Select(s => s.CheckOutCount.Value).FirstOrDefault();
                    //ScheduledCount = reportsLists.Select(s => s.CheckOutCount.Value).FirstOrDefault();
                }
                else
                {
                    reportsLists = reportService.GetWeeklyReportData(model, pageStart, pageEnd, sortColumn, sortorder);
                }

                recordsTotal = reportsLists.Select(s => s.TotalCount.Value).FirstOrDefault();

                var response = reportsLists.ToList();
                return Json(new
                {
                    response = response,
                    pageEnd = pageEnd,
                    pageStart = pageStart,
                    TotalRecords = recordsTotal,
                    CheckInCount = checkInCount,
                    CheckOutCount = checkOutCount,
                    ScheduledCount = ScheduledCount
                }, new Newtonsoft.Json.JsonSerializerSettings());
            }

        }

    }
}