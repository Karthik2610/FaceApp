using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FaceApp.Models;
using FaceApp.Services;
using FaceApp.Models.ViewModels;
using System.IO;
using FaceApp.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using FaceApp.Attributes;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Linq;

namespace FaceApp.Controllers
{
    public class SettingsController : Controller
    {
        private readonly FaceAppDBContext _context;
        private static readonly FaceAppDBContext _Stcontext;
        static SettingsService settingsService = null;
        //static Logger log = new Logger();
        //private readonly IHostingEnvironment _environment;
        const int CallLimitPerSecond = 10;        
        //Person[] persons;
        static Queue<DateTime> _timeStampQueue = new Queue<DateTime>(CallLimitPerSecond);
        //private readonly ILogger<PersonController> _logger;
        public SettingsController(FaceAppDBContext context, ILogger<PersonController> logger)
        {
            _context = context;
            settingsService = new SettingsService(context);
            //_logger = logger;
        }       
        static class ConfigurationManager
        {
            public static IConfiguration AppSetting { get; }
            static ConfigurationManager()
            {
                AppSetting = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json")
                        .Build();
            }
        }


        [NoDirectAccess]
        public IActionResult Index()
        {
            SettingsListViewModel model = new SettingsListViewModel();

            model.LoginUserId = Int32.Parse(HttpContext.Session.GetString("LoginUserId"));
            model.IsAdmin = bool.Parse(HttpContext.Session.GetString("LoginIsAdmin"));

            //model.settingsList = settingsService.GetSettingsList();
            return View(model);
        }

        [HttpPost]
        public ActionResult LoadData(SettingsListViewModel model, string pagestart = null, string pageend = null, string sortColumn = null, string sortOrder = null)
        {
            int pageStart = pagestart != null ? Convert.ToInt32(pagestart) : 0;
            int pageEnd = pageend != null ? Convert.ToInt32(pageend) : 0;
            int recordsTotal = 0;
            int sortorder = 0;
            if (sortOrder == "desc")
            {
                sortorder = 1;
            }

            using (FaceAppDBContext dc = new FaceAppDBContext())
            {
                var CalendarsLists = settingsService.GetSettingsList(model, pageStart, pageEnd, sortColumn, sortorder);
                recordsTotal = CalendarsLists.Select(s => s.TotalCount).FirstOrDefault();

                var response = CalendarsLists.ToList();
                return Json(new { response = response, pageEnd = pageEnd, pageStart = pageStart, TotalRecords = recordsTotal }, new Newtonsoft.Json.JsonSerializerSettings());
            }
        }

        [NoDirectAccess]
        public IActionResult Create(int? CalendarId)
        {
            SettingsViewModel Model = new SettingsViewModel();
            //Model.ServiceAssignments = new List<ServiceAssignmentsViewModel>();
            if (CalendarId.HasValue && CalendarId.Value > 0)
            {
                Model = settingsService.GetCalendarData(CalendarId.Value);
            }
            else
            {
                Model.IsActive = true;
            }
            return View(Model);
        }

        [HttpPost]
        public IActionResult Create(SettingsViewModel model)
        {

            ValidateCalendarModel(model);
            if (ModelState.IsValid)
            {

                try
                {
                    model.LoginUserId = int.Parse(HttpContext.Session.GetString("LoginUserId"));
                    
                    if (model.CalendarId > 0)
                    {
                        settingsService.UpdateCalendar(model);
                    }
                    else
                    {
                        settingsService.CreateCalendar(model);
                    }
                }
                catch
                {
                    return RedirectToAction("Login", "Account");
                }

            }
            else
            {
                return View(model);
            }

            return RedirectToAction("Index", "Settings");
        }

        private void ValidateCalendarModel(SettingsViewModel model)
        {
            if (model.Name != null && !string.IsNullOrEmpty(model.Name.Trim()))
            {
                CalendarSetting calendarSetting = settingsService.GetCalendarByName(model);
                if (calendarSetting != null)
                {
                    ModelState.AddModelError("Name", "Calendar Name "+ model.Name +" already exists");
                }
            }

            if (!model.SundayIsActive && !model.MondayIsActive && !model.TuesdayIsActive && !model.WednesdayIsActive
                && !model.ThursdayIsActive && !model.FridayIsActive && !model.SaturdayIsActive)
            {
                ModelState.AddModelError("spn_Error", "Please select any one of the days in the calendar details");
            }

            if (model.SundayEndTime != null && model.SundayStartTime != null)
            {
                var SundayEndTime = Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.SundayEndTime);
                var SundayStartTime = Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.SundayStartTime);
                if (SundayStartTime > SundayEndTime)
                {
                    ModelState.AddModelError("SundayEndTime", "Sunday End Time must greated than Sunday Start Time");
                }
            }
            if (model.MondayEndTime != null && model.MondayStartTime != null)
            {
                var MondayEndTime = Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.MondayEndTime);
                var MondayStartTime = Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.MondayStartTime);
                if (MondayStartTime > MondayEndTime)
                {
                    ModelState.AddModelError("MondayEndTime", "Monday End Time must greated than Monday Start Time");
                }
            }
            if (model.TuesdayEndTime != null && model.TuesdayStartTime != null)
            {
                var TuesdayEndTime = Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.TuesdayEndTime);
                var TuesdayStartTime = Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.TuesdayStartTime);
                if (TuesdayStartTime > TuesdayEndTime)
                {
                    ModelState.AddModelError("TuesdayEndTime", "Tuesday End Time must greated than Tuesday End Time");
                }
            }
            if (model.WednesdayEndTime != null && model.WednesdayStartTime != null)
            {
                var WednesdayEndTime = Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.WednesdayEndTime);
                var WednesdayStartTime = Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.WednesdayStartTime);
                if (WednesdayStartTime > WednesdayEndTime)
                {
                    ModelState.AddModelError("WednesdayEndTime", "Wednesday End Time must greated than Wednesday Start Time");
                }
            }
            if (model.ThursdayEndTime != null && model.ThursdayStartTime != null)
            {
                var ThursdayEndTime = Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.ThursdayEndTime);
                var ThursdayStartTime = Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.ThursdayStartTime);
                if (ThursdayStartTime > ThursdayEndTime)
                {
                    ModelState.AddModelError("ThursdayEndTime", "Thursday End Time must greated than Thursday Start Time");
                }
            }
            if (model.FridayEndTime != null && model.FridayStartTime != null)
            {
                var FridayEndTime = Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.FridayEndTime);
                var FridayStartTime = Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.FridayStartTime);
                if (FridayStartTime > FridayEndTime)
                {
                    ModelState.AddModelError("FridayEndTime", "Friday End Time must greated than Friday Start Time");
                }
            }
            if (model.SaturdayEndTime != null && model.SaturdayStartTime != null)
            {
                var SaturdayEndTime = Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.SaturdayEndTime);
                var SaturdayStartTime = Convert.ToDateTime(new DateTime(2020, 01, 01).ToString("dd/MMM/yyyy") + " " + model.SaturdayStartTime);
                if (SaturdayStartTime > SaturdayEndTime)
                {
                    ModelState.AddModelError("SaturdayEndTime", "Saturday End Time must greated than Saturday Start Time");
                }
            }
        }
    }
}
