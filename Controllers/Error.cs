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
    public class ErrorController : Controller
    {
        private readonly FaceAppDBContext _context;
        private static readonly FaceAppDBContext _Stcontext;
        static UsersService usersService = null;
        
        public ErrorController(FaceAppDBContext context)
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
        public IActionResult Unauthorized()
        {
            return View();
        }

    }
}
