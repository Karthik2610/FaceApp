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
//using FaceApp.LogUtility;
using System.Linq;
using Microsoft.AspNetCore.Http;
using FaceApp.Attributes;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Extensions.Configuration;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FaceApp.Controllers
{
    public class PersonController : Controller
    {
        string SUBSCRIPTION_KEY = ConfigurationManager.AppSetting["AppSettings:SubscriptionKey"];         
        string ENDPOINT = ConfigurationManager.AppSetting["AppSettings:Endpoint"];        
        string personGroupId = ConfigurationManager.AppSetting["AppSettings:PersonGroupId"];
        private readonly FaceAppDBContext _context;
        private static readonly FaceAppDBContext _Stcontext;
        static PersonService personService = null;
        static FaceService service = null;        
        static Person[] persons = new Person[1];
        //static Logger log = new Logger();
        //private readonly IHostingEnvironment _environment;
        const int PersonCount = 1;
        const int CallLimitPerSecond = 10;        
        //Person[] persons;
        static Queue<DateTime> _timeStampQueue = new Queue<DateTime>(CallLimitPerSecond);
        //private readonly ILogger<PersonController> _logger;
        public PersonController(FaceAppDBContext context, ILogger<PersonController> logger)
        {
            _context = context;
            personService = new PersonService(context);
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
        public IActionResult Attendance()
        {
            int? LoginUserId = 0;
            bool IsAdmin = false;
            try
            {
                LoginUserId = Int32.Parse(HttpContext.Session.GetString("LoginUserId"));
                IsAdmin = bool.Parse(HttpContext.Session.GetString("LoginIsAdmin"));
            }
            catch
            {
                return RedirectToAction("Login", "Account");
            }

            Face model= new Face();
            model.AttendanceType = "CheckIn";
            ViewBag.IsError = false;
            LoadAttendanceDropDownList(model, LoginUserId, IsAdmin);
            if(model.ProgramList.Count > 0) { model.ProgramList[0].Selected = true; } // program dropdown and make it default by selecting first value.
			if (model.LocationList.Count == 1) { model.LocationList[0].Selected = true; } // Location dropdown value selected by default if contains one value


            var LocationId = TempData["LocationId"];
            var ProgramId = TempData["ProgramId"];
            if (HttpContext.Session.GetString("TransitNumber") != null)
            {
                model.TransitNumber = HttpContext.Session.GetString("TransitNumber");
            }

            if (LocationId != null && !string.IsNullOrEmpty(LocationId.ToString()))
            {
                model.LocationId = Convert.ToInt32(LocationId);
            }
            if (ProgramId != null && !string.IsNullOrEmpty(ProgramId.ToString()))
            {
                model.ProgramId = Convert.ToInt32(ProgramId);
            }

            return View(model);
        }
        [NoDirectAccess]
        public IActionResult Train()
        {
            persons = new Person[1];
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Attendance([Bind("PersonName,CapturedFile,UploadedFile,Modeoftransportation,AttendanceType,LocationId,ProgramId,CheckOutDate,LastCheckInDate,IsContinueWithCheckOut,TransitNumber")] Face model, string browerName)
        {
            int LoginUserId = 0;
            bool IsAdmin = false;
            try
            {
                LoginUserId = int.Parse(HttpContext.Session.GetString("LoginUserId"));
                IsAdmin = bool.Parse(HttpContext.Session.GetString("LoginIsAdmin"));

            }
            catch (Exception ex)
            {
                return RedirectToAction("Login", "Account");
            }
            validateFaceModelCheckOutDate(model);
            if (ModelState.IsValid)
            {
                if(model.TransitNumber != null && !string.IsNullOrEmpty(model.TransitNumber.Trim()))
                {
                    HttpContext.Session.SetString("TransitNumber", model.TransitNumber.Trim());
                }
                else
                {
                    HttpContext.Session.SetString("TransitNumber", "");
                }

                TempData["LocationId"] = model.LocationId;
                TempData["ProgramId"] = model.ProgramId;
                ViewBag.browerName = browerName;
                //string path = ConfigurationManager.AppSetting["AppSettings:Faces"];
                string[] strfiles = new string[1];
                //model.CapturedFile = "Test";
                if (!string.IsNullOrEmpty(model.CapturedFile))
                {
                    //byt = System.Text.Encoding.UTF8.GetBytes(model.CapturedFile);
                    //var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                    //var filepath = path + $@"{"Identify_" + myUniqueFileName + ".png"}";



                    //System.IO.File.WriteAllBytes(filepath, bytes);
                    //strfiles[0] = filepath;
                    //}
                    ViewBag.imgcap = model.CapturedFile;
                    string vmsg = "";
                    //foreach (string file in strfiles)
                    //{
                    //if (file != null && file != "")                    
                    //{
                    
                    string base64 = model.CapturedFile.Split(',')[1];
                    byte[] bytes = Convert.FromBase64String(base64);
                    //string fpath = @"E:\Suganeswaran.G\Ajith.jpg";
                    //byte[] bytes = System.IO.File.ReadAllBytes(fpath);
                    await IdentifyPerson(model.PersonName, bytes, _context, vmsg, LoginUserId, model.Modeoftransportation,model);

                    if (ViewBag.Result != null)
                    {
                        vmsg = ViewBag.Result;
                    }

                    if (ViewBag.LastCheckInDate != null)
                    {
                        model.LastCheckInDate = ViewBag.LastCheckInDate;
                    }
                    //}
                    //}
                }
            }
            if(ViewBag.IsError == null) { ViewBag.IsError = false; };
            LoadAttendanceDropDownList(model, LoginUserId,IsAdmin);
            model.CapturedFile = "";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Train([Bind("PersonName,CapturedFile,UploadedFile")] Face model)
        {
            //string path = ConfigurationManager.AppSetting["AppSettings:Faces"];
            string[] strfiles = new string[2];
            if (string.IsNullOrEmpty(model.PersonName))
            {
                ViewBag.Error = "Please enter the Person Name";
                return View();
            }
            int cnt = 0;
            if (!string.IsNullOrEmpty(model.CapturedFile))
            {
                //byt = System.Text.Encoding.UTF8.GetBytes(model.CapturedFile);
                //var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                //var filepath = path + $@"{myUniqueFileName + ".png"}";

                string base64 = model.CapturedFile.Split(',')[1];
                byte[] bytes = Convert.FromBase64String(base64);

                //System.IO.File.WriteAllBytes(filepath, bytes);
                //strfiles[0] = filepath;
                //}
                //var files = HttpContext.Request.Form.Files;
                //if (files != null)
                //{
                //    foreach (var file in files)
                //    {
                //        if (file.Length > 0)
                //        {
                //            // Getting Filename  
                //            var fileName = file.FileName;
                //            // Unique filename "Guid"  
                //            var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                //            // Getting Extension  
                //            var fileExtension = Path.GetExtension(fileName);
                //            if (fileExtension == "")
                //            {
                //                fileExtension = "png";
                //            }

                //            var newFileName = string.Concat(myUniqueFileName, fileExtension);
                //            //  Generating Path to store photo   
                //            var filepath = path + $@"{newFileName}";

                //            if (!string.IsNullOrEmpty(filepath))
                //            {
                //                // Storing Image in Folder  
                //                StoreInFolder(file, filepath);
                //                strfiles[1] = filepath;
                //            }

                //        }
                //    }
                //}
                string vmsg = "";                
                //foreach (string file in strfiles)
                //{
                //if (file != null && file != "")
                //{
                cnt++;
                await CreatePerson(model.PersonName, null, bytes, _context, vmsg);
                if (ViewBag.Success != null)
                {
                    vmsg = ViewBag.Success;
                }
                //}
                //}
                if (cnt == 0)
                {
                    ViewBag.Error = "Please take a snap shot or upload an image";
                }
            }
            return View();
        }


        [NoDirectAccess]
        public IActionResult Index()
        {
            PersonsListViewModel model = new PersonsListViewModel();

            model.LoginUserId = Int32.Parse(HttpContext.Session.GetString("LoginUserId"));
            model.IsAdmin = bool.Parse(HttpContext.Session.GetString("LoginIsAdmin"));

            //model.personsList = personService.GetPersonsList();
            return View(model);
        }

        [HttpPost]
        public ActionResult LoadData(PersonsListViewModel model, string pagestart = null, string pageend = null, string sortColumn = null, string sortOrder = null)
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
                var PersonsLists = personService.GetPersonsList(model, pageStart, pageEnd, sortColumn, sortorder);
                recordsTotal = PersonsLists.Select(s => s.TotalCount).FirstOrDefault();

                var response = PersonsLists.ToList();
                return Json(new { response = response, pageEnd = pageEnd, pageStart = pageStart, TotalRecords = recordsTotal }, new Newtonsoft.Json.JsonSerializerSettings());
            }
        }

        [NoDirectAccess]
        public IActionResult Create(int? PersonId)
        {
            PersonsViewModel Model = new PersonsViewModel();
            Model.ServiceAssignments = new List<ServiceAssignmentsViewModel>();
            if (PersonId.HasValue && PersonId.Value > 0)
            {
                Model = personService.GetPersonsData(PersonId.Value);
            }
            else
            {
                Model.PersonDetailsList = new List<PersonDetailsListViewModel>();
                Model.IsActive = true;
            }
            LoadDropDownList(Model);
            return View(Model);
        }

        private void LoadAttendanceDropDownList(Face model,int?  LoginUserId,bool IsAdmin)
        {
            model.LocationList = personService.GetLocationsList(null, LoginUserId, IsAdmin);
            if (IsAdmin){
                model.ProgramList = personService.GetProgramsList(null);
            }
            else {
                model.ProgramList = personService.GetProgramsList(null, LoginUserId);
            }
        }

        private void LoadDropDownList(PersonsViewModel model)
        {
            model.LocationsList = personService.GetLocationsList(null);
            model.ProgramsList = new List<SelectListItem>();
            model.CalendarsList = new List<SelectListItem>();
        }

        [HttpPost]
        public async Task<IActionResult> Create(PersonsViewModel model)
        {
         

            if (ModelState.IsValid)
            {
                try
                {
                    model.LoginUserId = int.Parse(HttpContext.Session.GetString("LoginUserId"));
                }
                catch
                {
                    return RedirectToAction("Login", "Account");
                }

                int PersonId = 0;
                if (model.PersonId > 0)
                {
                    personService.UpdatePerson(model);
                }
                else
                {
                    personService.CreatePerson(model);
                }

                //string path = ConfigurationManager.AppSetting["AppSettings:Faces"];
                //string[] strfiles = new string[2];
                if (string.IsNullOrEmpty(model.FirstName) && string.IsNullOrEmpty(model.LastName))
                {
                    //log.Error("test");
                    ViewBag.Error = "Please enter the Person Name";
                    return View();
                }
                int cnt = 0;
                if (!string.IsNullOrEmpty(model.CapturedFile) || model.UploadedFile!=null)
                {
                    //byt = System.Text.Encoding.UTF8.GetBytes(model.CapturedFile);
                    //   var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                    //   var filepath = path + $@"{myUniqueFileName + ".png"}";
                    string base64 = "";
                    if (!string.IsNullOrEmpty(model.CapturedFile))
                    {
                        base64 = model.CapturedFile.Split(',')[1];
                    }
                    else if (model.UploadedFile != null)
                    {
                        var file = HttpContext.Request.Form.Files[0];
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            var fileBytes = ms.ToArray();
                            base64 = Convert.ToBase64String(fileBytes);                            
                        }                        
                    }
                    byte[] bytes = Convert.FromBase64String(base64);

                    //    System.IO.File.WriteAllBytes(filepath, bytes);
                    //    strfiles[0] = filepath;

                    //var files = HttpContext.Request.Form.Files;
                    //if (files != null)
                    //{
                    //    foreach (var file in files)
                    //    {
                    //        if (file.Length > 0)
                    //        {
                    //            // Getting Filename  
                    //            var fileName = file.FileName;
                    //            // Unique filename "Guid"  
                    //            var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                    //            // Getting Extension  
                    //            var fileExtension = Path.GetExtension(fileName);
                    //            if (fileExtension == "")
                    //            {
                    //                fileExtension = "png";
                    //            }

                    //            var newFileName = string.Concat(myUniqueFileName, fileExtension);
                    //            //  Generating Path to store photo   
                    //            var filepath = path + $@"{newFileName}";

                    //            if (!string.IsNullOrEmpty(filepath))
                    //            {
                    //                // Storing Image in Folder  
                    //                StoreInFolder(file, filepath);
                    //                strfiles[1] = filepath;
                    //            }

                    //        }
                    //    }
                    //}
                    string vmsg = "";

                    //foreach (string file in strfiles)
                    //{
                    //if (file != null && file != "")
                    //{
                    cnt++;
                    if (!model.IsClearTrain)
                    {
                        await CreatePerson(model.LastName + ' ' + model.FirstName, model.PersonId, bytes, _context, vmsg);
                    }
                    else
                    {
                        await RemovePersonFace(model.LastName + ' ' + model.FirstName, model.PersonId, bytes, _context, vmsg);
                        await CreatePerson(model.LastName + ' ' + model.FirstName, model.PersonId, bytes, _context, vmsg);
                    }
                    if (ViewBag.Success != null)
                    {
                        vmsg = ViewBag.Success;
                    }
                    //}
                    //}
                    if (cnt == 0)
                    {
                        ViewBag.Error = "Please take a snap shot or upload an image";
                    }
                }
                //return View(model);
                return RedirectToAction("Create", "Person",new { PersonId = model.PersonId});

            }
            else
            {
                LoadDropDownList(model);
                return View(model);
            }
            
            //return RedirectToAction("Index", "Person");
        }



        static async Task WaitCallLimitPerSecondAsync()
        {
            Monitor.Enter(_timeStampQueue);
            try
            {
                if (_timeStampQueue.Count >= CallLimitPerSecond)
                {
                    TimeSpan timeInterval = DateTime.UtcNow - _timeStampQueue.Peek();
                    if (timeInterval < TimeSpan.FromSeconds(1))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1) - timeInterval);
                    }
                    _timeStampQueue.Dequeue();
                }
                _timeStampQueue.Enqueue(DateTime.UtcNow);
            }
            finally
            {
                Monitor.Exit(_timeStampQueue);
            }
        }
        public static IFaceClient Authenticate(string endpoint, string key)
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }
        private async Task CreatePerson(string personname,int? personId, byte[] bytes, FaceAppDBContext context, string vmsg)
        {
            Guid personid = new Guid();
            IFaceClient client = Authenticate(ENDPOINT, SUBSCRIPTION_KEY);
            byte[] buff = bytes;//System.IO.File.ReadAllBytes(filepath);
            System.IO.MemoryStream ms = new System.IO.MemoryStream(buff);
            string viewbg = "";

            //if (persons[0] == null)
            //{
                List<PersonTrainDetails> per = context.PersonTrainDetails.Where(p => p.PersonId == personId.Value).ToList();
                if (per.Count == 0)
                {
                    await WaitCallLimitPerSecondAsync();
                    await Task.Delay(250);
                    persons[0] = await client.PersonGroupPerson.CreateAsync(personGroupId: personGroupId, name: personname);
                    personid = persons[0].PersonId;
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            PersonTrainDetails personTrainDetails = new PersonTrainDetails();
                            personTrainDetails.PersonTrainId = persons[0].PersonId;
                            personTrainDetails.PersonId = personId.Value;
                            await context.PersonTrainDetails.AddAsync(personTrainDetails);
                            await context.SaveChangesAsync();
                            transaction.Commit();
                            viewbg = "Created a person";
                        }
                        catch (Exception e)
                        {   //_logger.LogError("Error while creating a person");
                            transaction.Rollback();
                            viewbg = "Error while creating a person";
                            return;
                        }
                    }
                }
                else
                {
                    personid = per[0].PersonTrainId;
                    if (vmsg == "")
                    {
                        viewbg = "Person already exists so added a face to that person";
                    }
                    else
                    {
                        viewbg = vmsg;
                    }
                }
            //}

            try
            {
                await WaitCallLimitPerSecondAsync();
                await Task.Delay(250);
                await client.PersonGroupPerson.AddFaceFromStreamWithHttpMessagesAsync(personGroupId, personid, ms, detectionModel: DetectionModel.Detection03);

                await client.PersonGroup.TrainAsync(personGroupId);

                // Wait until the training is completed.
                while (true)
                {
                    await Task.Delay(1000);
                    var trainingStatus = await client.PersonGroup.GetTrainingStatusAsync(personGroupId);
                    Console.WriteLine($"Training status: {trainingStatus.Status}.");
                    if (trainingStatus.Status == TrainingStatusType.Succeeded) {
                        PersonTrainDetails PersonDetails = context.PersonTrainDetails.FirstOrDefault(p => p.PersonId == personId.Value);
                        if(PersonDetails != null)
                        {
                            PersonDetails.ModifiedDate = System.DateTime.Now;
                            PersonDetails.NoOfTrainings = ((PersonDetails.NoOfTrainings == null) ? 0 : PersonDetails.NoOfTrainings) + 1;
                            await _context.SaveChangesAsync();                           
                        }
                        break;
                    }
                }
                ViewBag.Success = viewbg;
            }
            catch (Exception e)
            {
                //_logger.LogError("Error while adding face to a person");
                ViewBag.Error = "Error while adding face to a person";
                return;
            }
        }

        private async Task RemovePersonFace(string personname, int? PersonId, byte[] bytes, FaceAppDBContext context, string vmsg)
        {
            IFaceClient client = Authenticate(ENDPOINT, SUBSCRIPTION_KEY);
            byte[] buff = bytes;// System.IO.File.ReadAllBytes(filepath);
            System.IO.MemoryStream ms = new System.IO.MemoryStream(buff);
            string viewbg = "";


            // Detect faces from source image url.

            var PersonTrainDetails = (from ptd in _context.PersonTrainDetails
                                      join p in _context.Persons on ptd.PersonId equals p.PersonId
                                      where ptd.PersonId == PersonId
                                      select ptd.PersonTrainId
                    ).ToList();


            // Add detected faceId to sourceFaceIds.
            foreach (var PersonTrainId in PersonTrainDetails) {
                    // Identify the faces in a person group. 
                    if (PersonTrainId != null)
                    {
                        var identifyResults = await client.PersonGroupPerson.GetAsync(personGroupId, PersonTrainId);

                        foreach (var FaceId in identifyResults.PersistedFaceIds)
                        {
                            await client.PersonGroupPerson.DeleteFaceAsync(personGroupId, PersonTrainId, FaceId);
                        }
                        ViewBag.TryAgain = "";
                    }
                    else
                    {
                        viewbg = $"Oops! identity not matched!, try again";
                        ViewBag.TryAgain = "TryAgain";
                    }
                    ViewBag.Result = viewbg;
            }

            
        }


        private async Task IdentifyPerson(string personname, byte[] bytes, FaceAppDBContext context, string vmsg,int LoginUserId,string Modeoftransportation,Face face)
        {
            IFaceClient client = Authenticate(ENDPOINT, SUBSCRIPTION_KEY);
            byte[] buff = bytes;//System.IO.File.ReadAllBytes(filepath);
            System.IO.MemoryStream ms = new System.IO.MemoryStream(buff);
            string viewbg = "";

            IList<Guid> sourceFaceIds = new List<Guid>();
            // Detect faces from source image url.
            IList<DetectedFace> detectedFaces = await client.Face.DetectWithStreamAsync(ms,
                                    recognitionModel: RecognitionModel.Recognition04, detectionModel: DetectionModel.Detection03);

            // Add detected faceId to sourceFaceIds.
            foreach (var detectedFace in detectedFaces) { sourceFaceIds.Add(detectedFace.FaceId.Value); }

            // Identify the faces in a person group. 
            if (sourceFaceIds != null && sourceFaceIds.Count > 0)
            {
                var identifyResults = await client.Face.IdentifyAsync(sourceFaceIds, personGroupId);

                foreach (var identifyResult in identifyResults)
                {
                    if (identifyResult.Candidates != null && identifyResult.Candidates.Count > 0)
                    {
                        Person person = await client.PersonGroupPerson.GetAsync(personGroupId, identifyResult.Candidates[0].PersonId);
                        /*viewbg= $"Person '{person.Name}' is identified for face in:'{identifyResult.FaceId}'," +
                            $" confidence: {identifyResult.Candidates[0].Confidence}.";*/

                        // User name 
                        var PersonName = person.Name;
                        var IsError = false;
                        if (person.PersonId != null)
                        {
                            int PersonId = (from ptd in _context.PersonTrainDetails
                                            join p in _context.Persons on ptd.PersonId equals p.PersonId
                                            where ptd.PersonTrainId == person.PersonId
                                            select p.PersonId
                             ).FirstOrDefault();

                         
                            personService = new PersonService(_context);
                            
                            Persons dbperson = personService.GetPersonById(PersonId);
                            if (dbperson != null)
                            {
                                    IsError = validateAttendance(dbperson, LoginUserId, face, PersonName);
                                    if (!IsError && (ViewBag.IsSuccess == null || !ViewBag.IsSuccess))
                                    {
                                        personService.UpdateAttendance(dbperson.PersonId, Modeoftransportation, face.AttendanceType, face.LocationId, face.ProgramId, face.TransitNumber, LoginUserId);
                                        PersonName = dbperson.LastName + ' ' + dbperson.FirstName;
                                    }
                            }
                            else { PersonName = null; }
                        }
                        if (!string.IsNullOrEmpty(PersonName) && !IsError)
                        {
                            if (face.AttendanceType == "CheckOut")
                            {
                                viewbg = $"{PersonName} You have been checked-out successfully. Thank you for attending.";
                            }
                            else
                            {
                                viewbg = $"Thank you {PersonName}! Welcome, it is nice to see you!";
                            }
                            ViewBag.TryAgain = "";
                        }
                        else if(!IsError)
                        {
                            viewbg = $"Oops! identity not matched!, try again";
                            ViewBag.TryAgain = "TryAgain";
                        }
                        else if (IsError && ViewBag.ErrorOn == "Program Not Exists")
                        {
                            viewbg = $"Oops! Program not assigned to you";
                            ViewBag.TryAgain = "TryAgain";
                        }
                        else if (IsError && ViewBag.ErrorOn == "Program Schedule Not Exists")
                        {
                            viewbg = $"Oops! Program assigned to you but not scheduled Today/Current Time";
                            ViewBag.TryAgain = "TryAgain";
                        }
                        else if (IsError && ViewBag.ErrorOn == "Already Check-In")
                        {
                            viewbg = $"Oops! you already Checked-in unable to Check-In again";
                            ViewBag.TryAgain = "TryAgain";
                        }
                        else if (IsError && ViewBag.ErrorOn == "Already Check-Out")
                        {
                            viewbg = $"Oops! you already Checked-Out unable to Checked-Out again";
                            ViewBag.TryAgain = "TryAgain";
                        }
                        else
                        {                            
                            if (ViewBag.TryAgain ==null || ViewBag.TryAgain == "") //block update tryagain for last no checkin alert
                            {
                                ViewBag.TryAgain = "";
                                viewbg = null;
                            }
                      
                        }
                    }
                    else
                    {
                        viewbg = $"Oops! identity not matched!, try again";
                        ViewBag.TryAgain = "TryAgain";
                    }
                }
            }
            else
            {
                viewbg = $"Oops! identity not matched!, try again";
                ViewBag.TryAgain = "TryAgain";
            }

            ViewBag.Result = viewbg;
        }

        private bool validateAttendance(Persons Person,int LoginUserId, Face face,string PersonName)
        {
            string GraceTime = ConfigurationManager.AppSetting["AppSettings:GraceTime"];
            bool Iserror = false;
            if (!face.IsContinueWithCheckOut)
            {
                bool IsLocationContains = personService.GetPersonLocationProgramDetails(Person.PersonId, face);

                if (IsLocationContains)
                {
                    bool IsLocationScheduleContains = false;
                    if (face.AttendanceType == "CheckIn")
                    {
                        IsLocationScheduleContains = personService.GetPersonLocationProgramScheduleDetails(Person.PersonId, face, Convert.ToInt32(GraceTime));
                    }
                    else
                    {
                        IsLocationScheduleContains = true;
                    }
                    if (IsLocationScheduleContains)
                    {

                        if (face.AttendanceType == "CheckIn")
                        {
                            bool IsAlreadyCheckIn = personService.GetTodayAttendaceCheckInStatus(Person.PersonId, face, Convert.ToInt32(GraceTime));
                            if (!IsAlreadyCheckIn)
                            {
                                bool checkOut = personService.GetAttendanceCheckOutStatus(Person.PersonId, face);

                                if (!checkOut)
                                {
                                    Attendance LastCheckIn = personService.GetLastCheckIn(Person.PersonId, face);
                                    Iserror = true;
                                    ViewBag.IsError = true;
                                    ViewBag.LastCheckInDate = LastCheckIn.AttendanceDate;
                                    ViewBag.ErrorOn = "LastCheckOut";
                                }
                            }
                            else
                            {
                                Iserror = true;
                                ViewBag.ErrorOn = "Already Check-In";
                                ViewBag.LastCheckInDate = null;
                            }
                        }
                        else
                        {
                            Attendance LastCheckIn = personService.GetLastCheckIn(Person.PersonId, face);
                            if(LastCheckIn != null && LastCheckIn.AttendanceId > 0)
                            {
                                face.LastCheckInDate = LastCheckIn.AttendanceDate;
                            }

                            if (face.CheckOutDate != null && (Convert.ToDateTime(face.CheckOutDate).ToShortDateString() != face.LastCheckInDate.Value.ToShortDateString()))
                            {
                                Iserror = true;
                                ViewBag.IsError = true;
                                ViewBag.ErrorOn = "LastCheckOut";
                                ViewBag.LastCheckInDate = face.LastCheckInDate;
                                ModelState.AddModelError("CheckOutDate", "Check-out date should be on the same day as Check-in date");
                            }
                            else{
                                bool IsAlreadycheckOut = personService.GetTodayAttendaceCheckOutStatus(Person.PersonId, face);
                                if (!IsAlreadycheckOut)
                                {
                                    //bool checkIn = personService.GetAttendanceCheckInStatus(PersonId.Value, face);
                                    bool checkIn = personService.GetLastAttendaceCheckInStatus(Person.PersonId, face);
                                    if (!checkIn)
                                    {
                                        Iserror = true;
                                        ViewBag.IsError = true;
                                        ViewBag.ErrorOn = "LastCheckIn";
                                        ViewBag.LastCheckInDate = null;
                                        ViewBag.TryAgain = "TryAgain";
                                    }
                                }
                                else
                                {
                                    Iserror = true;
                                    ViewBag.ErrorOn = "Already Check-Out";
                                    ViewBag.LastCheckInDate = null;
                                }
                            }
                        }
                    }
                    else
                    {
                        //program contains person but not schedule attendace date
                        Iserror = true;
                        ViewBag.ErrorOn = "Program Schedule Not Exists";
                        ViewBag.LastCheckInDate = null;
                    }
                }
                else
                {
                    Iserror = true;
                    ViewBag.ErrorOn = "Program Not Exists";
                    ViewBag.LastCheckInDate = null;
                }

            }
            else
            {
                if (Convert.ToDateTime(face.CheckOutDate).ToShortDateString() == face.LastCheckInDate.Value.ToShortDateString())
                {
                    Attendance LastCheckIn = personService.GetLastCheckIn(Person.PersonId, face);
                    personService.UpdateAttendance(Person.PersonId, face.Modeoftransportation, "CheckOut", LastCheckIn.LocationId, LastCheckIn.ProgramId, face.TransitNumber,LoginUserId, face.CheckOutDate);
                    if (face.AttendanceType == "CheckOut") {
                        PersonName = Person.LastName + ' ' + Person.FirstName;
                        ViewBag.IsSuccess = true;
                    } // return only Checkout for checkout action
                }
                else
                {
                    Iserror = true;
                    ViewBag.IsError = true;
                    ViewBag.ErrorOn = "LastCheckOut";
                    ViewBag.LastCheckInDate = face.LastCheckInDate;
                    ModelState.AddModelError("CheckOutDate", "Check-out date should be on the same day as Check-in date");
                }
            }
            return Iserror;
        }

        private void validateFaceModelCheckOutDate(Face face)
        {
            if (face.IsContinueWithCheckOut)
            {
                if (face.CheckOutDate != null && !string.IsNullOrEmpty(face.CheckOutDate))
                {

                    if (Convert.ToDateTime(face.CheckOutDate) < DateTime.Now)
                    {
                        if (Convert.ToDateTime(face.CheckOutDate) < face.LastCheckInDate)
                        {
							ViewBag.imgcap = face.CapturedFile;
							ViewBag.IsError = true;
                            ViewBag.LastCheckInDate = face.LastCheckInDate;
                            ViewBag.ErrorOn = "LastCheckOut";
                            ModelState.AddModelError("CheckOutDate", "Check-out date should be on the same day and time should be greater than Check-in date and time");
                        }
                    }
                    else
                    {
						ViewBag.imgcap = face.CapturedFile;
						ViewBag.IsError = true;
                        ViewBag.LastCheckInDate = face.LastCheckInDate;
                        ViewBag.ErrorOn = "LastCheckOut";
                        ModelState.AddModelError("CheckOutDate", "Check-Out Date should be less than current Date and Time ex:" + DateTime.Now);
                    }
                }
                else
                {
					ViewBag.imgcap = face.CapturedFile;
					ViewBag.IsError = true;
                    ViewBag.LastCheckInDate = face.LastCheckInDate;
                    ViewBag.ErrorOn = "LastCheckOut";
                    ModelState.AddModelError("CheckOutDate", "Check-Out Date is required");
                }
            }
        }
        
        private void StoreInFolder(IFormFile file, string fileName)
        {
            using (FileStream fs = System.IO.File.Create(fileName))
            {
                file.CopyTo(fs);
                fs.Flush();
            }
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ActionResult GetLocationSelectedData(int? LocationId)
        {
            var ProgramsLists = personService.GetProgramsList(LocationId);
            var result = new
            {
                programs = ProgramsLists,
            };

            return Json(new { result = result }, new Newtonsoft.Json.JsonSerializerSettings());
        }

        public ActionResult GetSelectedProgramData(int? ProgramId)
        {
            var CalendarLists = personService.GetCalendarList(ProgramId);

            var result = new
            {
                calendars = CalendarLists,
            };

            return Json(new { result = result }, new Newtonsoft.Json.JsonSerializerSettings());
        }

        //public ActionResult GetCalendarDuplicateStatus(int? CalendarId1, DateTime? C1StartDate, DateTime? C1EndDate, int? CalendarId2 , DateTime? C2StartDate, DateTime? C2EndDate)
        //{
        public ActionResult GetCalendarDuplicateStatus(List<CalendarDuplicateCheckModel> usertable)
        { 
            var status = personService.GetCalendarDuplicateStatus(usertable);

            var result = new
            {
                IsDuplicate = status,
            };

            return Json(new { result = result }, new Newtonsoft.Json.JsonSerializerSettings());
        }

        [HttpPost]
        public ActionResult GetPersonAttendaceStatus(int? PersonsDetailsId)
        {

            bool isProgramStarted = personService.GetPersonAttendaceStatus(PersonsDetailsId);

            return Json(new { IsProgramStarted = isProgramStarted }, new Newtonsoft.Json.JsonSerializerSettings());
        }

    }
}
