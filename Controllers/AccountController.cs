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
using Azure.Communication.Email;
using System.Net.Mail;
using System.Text;
using Azure;

namespace FaceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly FaceAppDBContext _context;
        private static readonly FaceAppDBContext _Stcontext;
        static UsersService usersService = null;
        Authentication authenticate = new Authentication();        
        EmailClient emailClient = new EmailClient(ConfigurationManager.AppSetting["AppSettings:EmailconnectionString"]);
        public AccountController(FaceAppDBContext context)
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
        public IActionResult Login()
        {
            LoginViewModel model = new LoginViewModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (model.Email == null || model.Email.ToString().Trim() == "")
            {
                ModelState.AddModelError("Email", "Enter Email");
            }
            if (model.Password == null || model.Password.ToString() == "")
            {
                ModelState.AddModelError("Password", "Enter Password");
            }

            if (ModelState.IsValid)
            {
                Users user = usersService.GetUserByEmail(model.Email.ToString().Trim());
                if (user != null && user.UserId > 0)
                {
                    string Passhash = authenticate.Hash(model.Password, user.Salt.ToString());
                    if (user.Password == Passhash)
                    {
                        HttpContext.Session.SetString("BackToIsAdmin", "");
                        HttpContext.Session.SetString("BackToUserId", "");
                        HttpContext.Session.SetString("ShowBackToLogin", "False");
                        HttpContext.Session.SetString("LoginIsAdmin", user.IsAdmin.ToString());
                        HttpContext.Session.SetString("LoginUserId", user.UserId.ToString());
                        HttpContext.Session.SetString("LoginWithTempPassword", user.IsForgotPassword.ToString());

                        if (user.IsForgotPassword)
                        {
                            return RedirectToAction("ChangePassword", "Account",new {IsLogin=true});
                        }
                        return RedirectToAction("Attendance", "Person");

                    }
                    else
                    {
                        ModelState.AddModelError("Email", "Invalid user credentials");
                        ModelState.AddModelError("Password", "Invalid user credentials");
                        //System.Diagnostics.Trace.TraceInformation("Password is Incorrect");
                    }

                }
                else
                {
                    ModelState.AddModelError("Password", "Invalid user credentials");
                    ModelState.AddModelError("Email", "Invalid user credentials");
                }

                if (!ModelState.IsValid) { return View(model); };

                return RedirectToAction("Index", "Users");
            }

            return View(model);
        }


        public IActionResult BackToLogin()
        {
            var BackToIsAdmin = HttpContext.Session.GetString("BackToIsAdmin");
            var BackToUserId = HttpContext.Session.GetString("BackToUserId");
            HttpContext.Session.SetString("ShowBackToLogin", "False");
            HttpContext.Session.SetString("LoginIsAdmin", BackToIsAdmin);
            HttpContext.Session.SetString("LoginUserId", BackToUserId);
            HttpContext.Session.SetString("BackToIsAdmin", "");
            HttpContext.Session.SetString("BackToUserId", "");

            return RedirectToAction("Attendance", "Person");
        }



        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }


        public IActionResult ForgotPassword()
        {
            ForgotPasswordModel model = new ForgotPasswordModel();
            return View(model);
        }


        public IActionResult ChangePassword(bool IsLogin = false)
        {
            ChangePasswordModel model = new ChangePasswordModel();
            model.isLoginRedirect = IsLogin;
            return View(model);
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordModel model)
        {
            //Session check    
                try
                {
                    model.LoginUserId = int.Parse(HttpContext.Session.GetString("LoginUserId"));
                }
                catch
                {
                    return RedirectToAction("Login", "Account");
                }
                //Session check
                Users user = usersService.GetUserById(model.LoginUserId);
                string oldpass = authenticate.Hash(model.OldPassword, user.Salt);
                if (user != null && user.Password == oldpass)
                {

                        if (model.Password != model.OldPassword)
                        { 
                            if (model.Password == model.NewPassword)
                                    {
                                        if (model.Password.Length < 21)
                                        {
                                            if (!usersService.ValidatePassword(model.Password))
                                            {
                                                ModelState.AddModelError("Password", "Password must contain at least one number, one uppercase, lowercase letter, special character, and at least 8 or more characters.");
                                            }
                                            else
                                            {
                                                model.Salt = authenticate.GenerateSaltValue();
                                                model.Password = authenticate.Hash(model.Password, model.Salt);
                                                model.UserId = user.UserId;
                                                usersService.UpdateUserPass(model);
                                                HttpContext.Session.SetString("LoginWithTempPassword", "False");
                                                return RedirectToAction("Attendance", "Person");
                                            }
                                        }
                                        else
                                        {
                                            ModelState.AddModelError("Password", "length must be  minimum 8 characters and maximum 20 characters.");
                                        }
                                    }
                                    else
                                    {
                                        ModelState.AddModelError("Password", "New Password and Confirm New Password  should be same.");
                                    }
                        }
                        else
                        {
                            ModelState.AddModelError("OldPassword", "Old Password and New Password should not be same.");
                        }
                 }
                else
                {
                    ModelState.AddModelError("OldPassword", "Wrong old Password");
                }

                return View(model);
        }



        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordModel model)
        {

            string pass, tmppass, name;
            model.Salt = authenticate.GenerateSaltValue();
            tmppass = authenticate.GeneratePassword(8);
            model.Password = authenticate.Hash(tmppass, model.Salt);
            name = "";

            if (model.Email != "")
            {
                Users user = usersService.GetUserByEmail(model.Email);
                if (user != null && user.UserId > 0)
                {
                    //Session check    
                    try
                    {
                        model.UserId = user.UserId;
                        usersService.UpdateUserPass(model);
                        name = user.LastName + " " + user.FirstName;

                        SendNotificationEmail(model.Email, name, tmppass);
                    }
                    catch
                    {
                        return RedirectToAction("Login", "Account");
                    }
                    //Session check
                }
                else
                {
                    ModelState.AddModelError("Email", "Invalid user credentials");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("Email", "Enter Email");
                return View(model);
            }

            return RedirectToAction("Login", "Account");
        }

        private void SendNotificationEmail(string emailid, string Name, string pass)
        {
            string FromAddress = ConfigurationManager.AppSetting["AppSettings:FromAddress"];
            string body = "";
            body+="<table style=\"font-size:12px;font-family:Arial;\">";
            body+="<tr><td>Hello " + Name + ",</td></tr>";
            body+="<tr><td></td></tr>";
            body+= "<tr><td>Your temporary Member Tracker password is:  " + pass + "</td></tr>";
            body+="<tr><td></td></tr>";
            body+= "<tr><td> Upon logging in to Member Tracker you will be allowed to change your temporary password to one of your choosing. Keep in mind that your new password requires a minimum of 8 characters.</td></tr>";
            body+="</table>";
            body+="<br />";
            body+="<span style=\"font-size:12px;font-family:Arial;\">Kind Regards,</span><br />";
            body+= "<span style=\"font-size:12px;font-family:Arial;\">Your Member Tracker Administrator</span>";
            try
            {
                var emailSendOperation = emailClient.Send(
                    wait: WaitUntil.Completed,
                    senderAddress: FromAddress,

                    recipientAddress: emailid,
            
                    subject: "Member Tracker Password Notification",
                    htmlContent: body);
                System.Diagnostics.Trace.TraceInformation($"Email Sent. Status = {emailSendOperation.Value.Status}");

                /// Get the OperationId so that it can be used for tracking the message for troubleshooting
                string operationId = emailSendOperation.Id;
                System.Diagnostics.Trace.TraceInformation($"Email operation id = {operationId}");
            }
            catch (RequestFailedException ex)
            {
                /// OperationID is contained in the exception message and can be used for troubleshooting purposes
                System.Diagnostics.Trace.TraceError($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
            }
        }        
    }
}
