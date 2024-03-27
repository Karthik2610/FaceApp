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
    public class ProgramController : Controller
    {
        private readonly FaceAppDBContext _context;
        private static readonly FaceAppDBContext _Stcontext;
        static ProgramService programService = null;
        //static Logger log = new Logger();
        //private readonly IHostingEnvironment _environment;
        const int CallLimitPerSecond = 10;        
        //Person[] persons;
        static Queue<DateTime> _timeStampQueue = new Queue<DateTime>(CallLimitPerSecond);
        //private readonly ILogger<PersonController> _logger;
        public ProgramController(FaceAppDBContext context, ILogger<PersonController> logger)
        {
            _context = context;
            programService = new ProgramService(context);
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
            ProgramListViewModel model = new ProgramListViewModel();

            model.LoginUserId = Int32.Parse(HttpContext.Session.GetString("LoginUserId"));
            model.IsAdmin = bool.Parse(HttpContext.Session.GetString("LoginIsAdmin"));

            //model.programList = programService.GetProgramList();
            return View(model);
        }

        [HttpPost]
        public ActionResult LoadData(ProgramListViewModel model, string pagestart = null, string pageend = null, string sortColumn = null, string sortOrder = null)
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
                var programsLists = programService.GetProgramList(model, pageStart, pageEnd, sortColumn, sortorder);
                recordsTotal = programsLists.Select(s => s.TotalCount).FirstOrDefault();

                var response = programsLists.ToList();
                return Json(new { response = response, pageEnd = pageEnd, pageStart = pageStart, TotalRecords = recordsTotal }, new Newtonsoft.Json.JsonSerializerSettings());
            }
        }

        [NoDirectAccess]
        public IActionResult Create(int? ProgramId)
        {
            ProgramViewModel Model = new ProgramViewModel();
            if (ProgramId.HasValue && ProgramId.Value > 0)
            {
                 Model = programService.GetProgramData(ProgramId.Value);
            }
            else
            {
                Model.ProgramCalendarList = new List<ProgramCalendarListViewModel>(); 
                Model.IsActive = true;
            }

            Model.CalendarList = programService.GetCalendarList();
            return View(Model);
        }

        [HttpPost]
        public IActionResult Create(ProgramViewModel model)
        {
            if (model.Name != null && !string.IsNullOrEmpty(model.Name.Trim()))
            {
                Programs programs = programService.GetProgramsByName(model);
                if (programs != null)
                {
                    ModelState.AddModelError("Name", "Program Name " + model.Name + " already exists");
                }
            }

            if (ModelState.IsValid)
            {

                try
                {
                    model.LoginUserId = int.Parse(HttpContext.Session.GetString("LoginUserId"));
                    
                    if (model.ProgramId > 0)
                    {
                        programService.UpdateProgram(model);
                    }
                    else
                    {
                        programService.CreateProgram(model);
                    }
                }
                catch
                {
                    return RedirectToAction("Login", "Account");
                }

            }
            else
            {
                model.CalendarList = programService.GetCalendarList();
                return View(model);
            }

            return RedirectToAction("Index", "Program");
        }

        [HttpPost]
        public ActionResult GetProgramCalendarSchedule(int? ProgramCalendarDetailsid)
        {

            bool isProgramCalendarSchedule = programService.GetProgramCalendarSchedule(ProgramCalendarDetailsid);

            return Json(new { IsProgramCalendarSchedule = isProgramCalendarSchedule }, new Newtonsoft.Json.JsonSerializerSettings());
        }
        

    }
}
