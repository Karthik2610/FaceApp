using System.Collections.Generic;
using System.Security.Claims;
using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using FaceApp.Data;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http;

namespace FaceApp.Attributes
{
    //NodirectAccess


    public class NoDirectAccessAttribute : ActionFilterAttribute, IActionFilter
    {

        public FaceAppDBContext _context = new FaceAppDBContext();
        public NoDirectAccessAttribute()
        {

        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
               Microsoft.AspNetCore.Http.HttpRequest Request = filterContext.HttpContext.Request;
               var url = Request.HttpContext.Request.GetEncodedUrl();
                //Direct Url Enter
                var LoginUserId = Request.HttpContext.Session.GetString("LoginUserId");
                var LoginIsAdmin = Request.HttpContext.Session.GetString("LoginIsAdmin");
                var LoginWithTempPassword = Request.HttpContext.Session.GetString("LoginWithTempPassword");
                
                var UrlLength = url.ToString().Split('/').Length;
                var RequestUrl = url.ToString().Split('/')[UrlLength - 1].ToString();
                var ActualUrl = url.ToString().Split('/')[UrlLength - 1].ToString();


                if (ActualUrl.StartsWith("Index") || ActualUrl.Contains("Reports") || ActualUrl.StartsWith("Create") || ActualUrl.StartsWith("Edit") || ActualUrl.StartsWith("View"))
                {
                    RequestUrl = url.ToString().Split('/')[UrlLength - 2].ToString();
                }

                if (UrlLength > 4 && RequestUrl == "") //Default Length 4  handle extra /
                {
                    RequestUrl = url.ToString().Split('/')[UrlLength - 2].ToString();
                    ActualUrl = url.ToString().Split('/')[UrlLength - 2].ToString();
                }

                if (LoginUserId != null)
                {
                    if (RequestUrl == "Users" && LoginIsAdmin == "False" ||
                       RequestUrl != "Account" && LoginWithTempPassword == "True")
                    {
                        filterContext.Result = new RedirectToRouteResult(new
                                                        RouteValueDictionary(new { controller = "Error", action = "Unauthorized", area = "Unauthorized" }));
                    }
                }
                    else
                {
                    filterContext.Result = new RedirectToRouteResult(new
                                                       RouteValueDictionary(new { controller = "Account", action = "Login", area = "Unauthorized" }));
                }

        }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    //NodirectAccess
}
