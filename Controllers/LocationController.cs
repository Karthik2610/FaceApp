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
using System.Linq;

namespace FaceApp.Controllers
{
    public class LocationController : Controller
    {
        private readonly FaceAppDBContext _context;
        private static readonly FaceAppDBContext _Stcontext;
        static LocationService locationService = null;
        //static Logger log = new Logger();
        //private readonly IHostingEnvironment _environment;
        const int CallLimitPerSecond = 10;        
        //Person[] persons;
        static Queue<DateTime> _timeStampQueue = new Queue<DateTime>(CallLimitPerSecond);
        //private readonly ILogger<PersonController> _logger;
        public LocationController(FaceAppDBContext context, ILogger<PersonController> logger)
        {
            _context = context;
            locationService = new LocationService(context);
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
            LocationListViewModel model = new LocationListViewModel();

            model.LoginUserId = Int32.Parse(HttpContext.Session.GetString("LoginUserId"));
            model.IsAdmin = bool.Parse(HttpContext.Session.GetString("LoginIsAdmin"));
            //model.LocationList = locationService.GetLocationList();
            return View(model);
        }

        [HttpPost]
        public ActionResult LoadData(LocationListViewModel model, string pagestart = null, string pageend = null, string sortColumn = null, string sortOrder = null)
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
                var locationsLists = locationService.GetLocationList(model, pageStart, pageEnd, sortColumn, sortorder);
                recordsTotal = locationsLists.Select(s => s.TotalCount).FirstOrDefault();

                var response = locationsLists.ToList();
                return Json(new { response = response, pageEnd = pageEnd, pageStart = pageStart, TotalRecords = recordsTotal }, new Newtonsoft.Json.JsonSerializerSettings());
            }
        }

        [NoDirectAccess]
        public IActionResult Create(int? LocationId)
        {
            LocationViewModel Model = new LocationViewModel();
            if (LocationId.HasValue && LocationId.Value > 0)
            {
                Model = locationService.GetLocationData(LocationId.Value);
            }
            else
            {
                Model.LocationProgramList = new List<LocationProgramListViewModel>(); 
                Model.IsActive = true;
            }

            Model.ProgramList = locationService.GetProgramList();
            return View(Model);
        }

        [HttpPost]
        public IActionResult Create(LocationViewModel model)
        {
            if (model.Name != null && !string.IsNullOrEmpty(model.Name.Trim())){
                Locations Location = locationService.GetLocationByName(model);
                if(Location != null)
                {
                    ModelState.AddModelError("Name", "Location Name " + model.Name + " already exists");
                }
              }

            if (ModelState.IsValid)
            {

                try
                {
                    model.LoginUserId = int.Parse(HttpContext.Session.GetString("LoginUserId"));
                    
                    if (model.LocationId > 0)
                    {
                        locationService.UpdateLocation(model);
                    }
                    else
                    {
                        locationService.CreateLocation(model);
                    }
                }
                catch
                {
                    return RedirectToAction("Login", "Account");
                }

            }
            else
            {
                model.ProgramList = locationService.GetProgramList();
                return View(model);
            }

            return RedirectToAction("Index", "Location");
        }

        [HttpPost]
        public ActionResult GetLocationProgramScheduleStatus(int? LocationProgramDetailsId)
        {

            bool isLocationProgramScheduled = locationService.GetLocationProgramScheduleStatus(LocationProgramDetailsId);

            return Json(new { IsLocationProgramScheduled = isLocationProgramScheduled }, new Newtonsoft.Json.JsonSerializerSettings());
        }

    }
}
