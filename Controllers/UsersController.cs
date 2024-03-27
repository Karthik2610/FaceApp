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
using FaceApp.Services;
using System.Linq;
using Microsoft.AspNetCore.Http;
using FaceApp.Attributes;

namespace FaceApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly FaceAppDBContext _context;
        private static readonly FaceAppDBContext _Stcontext;
        static UsersService usersService = null;
        
        public UsersController(FaceAppDBContext context)
        {
            _context = context;
            usersService = new UsersService(context);
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
            UsersListViewModel model = new UsersListViewModel();

            try
            {
                model.LoginUserId = Int32.Parse(HttpContext.Session.GetString("LoginUserId"));
                model.IsAdmin = bool.Parse(HttpContext.Session.GetString("LoginIsAdmin"));
            }
            catch
            {
                return RedirectToAction("Login", "Account");
            }

            //model.usersLists = usersService.GetUsersList();
            return View(model);
        }

        [HttpPost]
        public ActionResult LoadData(UsersListViewModel model, string pagestart = null, string pageend = null, string sortColumn = null, string sortOrder = null)
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
                var UsersLists = usersService.GetUsersList(model, pageStart, pageEnd, sortColumn, sortorder);
                recordsTotal = UsersLists.Select(s => s.TotalCount).FirstOrDefault();

                var response = UsersLists.ToList();
                return Json(new { response = response, pageEnd = pageEnd, pageStart = pageStart, TotalRecords = recordsTotal }, new Newtonsoft.Json.JsonSerializerSettings());
            }
        }

        [NoDirectAccess]
        public IActionResult Create(int? UserId)
        {
            UsersViewModel Model = new UsersViewModel();
            if (UserId.HasValue && UserId.Value > 0)
            {
                Model = usersService.GetUsersData(UserId.Value);
            }
            else{
                Model.UserLocationsList = new List<UserLocationsListViewModel>();
                Model.IsActive = true;
                Model.IsPasswordChanged = true;
            }
            Model.LocationsList = usersService.GetLocationsList();
            return View(Model);
        }
        //public IActionResult View(int? UserId)
        //{
        //    UsersViewModel Model = new UsersViewModel();
        //    if (UserId.HasValue && UserId.Value > 0)
        //    {
        //        Model=usersService.GetUsersData(UserId.Value);
        //    }
        //    return View(Model);
        //}
        
        [HttpPost]
        public IActionResult Create(UsersViewModel model)
        {
            if (model.Email != null && model.Email.Trim() != "") {
                 if (!usersService.ValidateEmail(model.Email))
                 {
                    ModelState.AddModelError("Email", "Invalid Email Address");
                 }
            }

            if (model.Password != null && model.Password.Trim() != "" && model.IsPasswordChanged == true)
            {
                if (model.Password.Length < 21)
                {
                    if (!usersService.ValidatePassword(model.Password))
                    {
                        ModelState.AddModelError("Password", "Password must contain at least one number and one uppercase and lowercase letter and special character , and at least 8 or more characters.");
                    }   
                }
                else
                {
                    ModelState.AddModelError("Password", "length must be minimum 8 characters and maximum 20 characters.");
                }
            }
            //else
            //{
            //    if (model.IsPasswordChanged) { ModelState.AddModelError("Password", "Enter Password"); }

            //}

            if (ModelState.IsValid)
            {

                try
                {
                    model.LoginUserId = int.Parse(HttpContext.Session.GetString("LoginUserId"));
                    if (model.UserId > 0)
                    {
                        usersService.UpdateUser(model);
                    }
                    else
                    {
                        usersService.CreateUser(model);
                    }
                }
                catch
                {
                    return RedirectToAction("Login", "Account");
                }

            }
            else
            {
                model.LocationsList = usersService.GetLocationsList();
                return View(model);
            }
            
            return RedirectToAction("Index", "Users");
        }


        public IActionResult ProxyLogin(int? UserId)
        {
            
                UsersViewModel Model = new UsersViewModel();

                if (UserId > 0)
                {
                    Users user = usersService.GetUserById(UserId.Value);
                   //Session Expired
                    try
                    {
                        var BackToLogin = HttpContext.Session.GetString("ShowBackToLogin");
                        var LoginIsAdmin = HttpContext.Session.GetString("LoginIsAdmin");
                        var LoginUserId = HttpContext.Session.GetString("LoginUserId");
                        if (BackToLogin != "True")
                        {
                            HttpContext.Session.SetString("ShowBackToLogin", "True");
                            HttpContext.Session.SetString("BackToIsAdmin", LoginIsAdmin);
                            HttpContext.Session.SetString("BackToUserId", LoginUserId);
                        }
                        HttpContext.Session.SetString("LoginIsAdmin", user.IsAdmin.ToString());
                        HttpContext.Session.SetString("LoginUserId", user.UserId.ToString());
                        HttpContext.Session.SetString("LoginWithTempPassword", "False"); //handling not showing change password
                    }
                    catch (Exception ex)
                    {
                        return RedirectToAction("Account", "Login");
                    }

                }
                return RedirectToAction("Attendance", "Person");
            
        }



        [HttpPost]
        public ActionResult GetLocationScheduleStatus(int? UserLocationDetailsId)
        {

            bool isLocationSchedule = usersService.GetLocationScheduleStatus(UserLocationDetailsId);

            return Json(new { IsLocationSchedule = isLocationSchedule }, new Newtonsoft.Json.JsonSerializerSettings());
        }



        //[HttpPost]
        //public ActionResult LoadData(string pagestart = null, string pageend = null, string sortColumn = null, string sortOrder = null)
        //{
        //    System.Diagnostics.Trace.TraceInformation("Users List LoadData Initial");
        //    int pageStart = pagestart != null ? Convert.ToInt32(pagestart) : 0;
        //    int pageEnd = pageend != null ? Convert.ToInt32(pageend) : 0;
        //    int recordsTotal = 0;
        //    int sortorder = 0;
        //    if (sortOrder == "desc")
        //    {
        //        sortorder = 1;
        //    }

        //    System.Diagnostics.Trace.TraceInformation("Before Users List LoadData");
        //    using (FaceAppDBContext dc = new FaceAppDBContext())
        //    {
        //        var usersLists = usersService.GetUsersList(pageStart, pageEnd, sortColumn, sortorder);
        //        //recordsTotal = usersLists.Select(s => s.TotalCount).FirstOrDefault();
        //        recordsTotal = usersLists.Count - 1;
        //        var response = usersLists.ToList();
        //        System.Diagnostics.Trace.TraceInformation("Completed Users List LoadData");
        //        return Json(new { response = response, pageEnd = pageEnd, pageStart = pageStart, TotalRecords = recordsTotal }, new Newtonsoft.Json.JsonSerializerSettings());
        //    }

        //}

    }
}
