using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FaceApp.Models;
using System.IO;
using FaceApp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System.Threading;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Extensions.Configuration;
using FaceApp.Services;
using System.Linq;

namespace FaceApp.Controllers
{
    public class HomeController : Controller
    {
        const string SUBSCRIPTION_KEY = "2690119676394fabb4cc50f596f8d58f";
        const string ENDPOINT = "https://aileron.cognitiveservices.azure.com/";
        private readonly FaceAppDBContext _context;
        private static readonly FaceAppDBContext _Stcontext;
        static FaceService service = null;
        static Person[] persons = new Person[1];
        //private readonly IHostingEnvironment _environment;
        const int PersonCount = 1;
        const int CallLimitPerSecond = 10;
        const string personGroupId = "aileron_employee";
        //Person[] persons;
        static Queue<DateTime> _timeStampQueue = new Queue<DateTime>(CallLimitPerSecond);
        

        //private readonly IFaceClient faceClient = new FaceClient(new ApiKeyServiceClientCredentials(SUBSCRIPTION_KEY),new System.Net.Http.DelegatingHandler[] { });

        public HomeController(FaceAppDBContext context)
        {
            _context = context;
            service = new FaceService(context);
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

        public IActionResult Users()
        {
            persons = new Person[1];
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Users([Bind("PersonName,CapturedFile,UploadedFile")] Face model)
        {            
            string path = ConfigurationManager.AppSetting["AppSettings:Faces"];
            string[] strfiles= new string[2];
            if (string.IsNullOrEmpty(model.PersonName))
            {
                ViewBag.Error = "Please enter the Person Name";
                return View();
            }
            if (!string.IsNullOrEmpty(model.CapturedFile))
            {
                //byt = System.Text.Encoding.UTF8.GetBytes(model.CapturedFile);
                var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                var filepath = path + $@"{myUniqueFileName+".png"}";

                string base64 = model.CapturedFile.Split(',')[1];
                byte[] bytes = Convert.FromBase64String(base64);
                
                System.IO.File.WriteAllBytes(filepath, bytes);
                strfiles[0] = filepath;
            }            
            var files = HttpContext.Request.Form.Files;            
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // Getting Filename  
                        var fileName = file.FileName;
                        // Unique filename "Guid"  
                        var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                        // Getting Extension  
                        var fileExtension = Path.GetExtension(fileName);
                        if (fileExtension == "")
                        {
                            fileExtension = "png";
                        }

                        var newFileName = string.Concat(myUniqueFileName, fileExtension);
                        //  Generating Path to store photo   
                        var filepath = path+ $@"{newFileName}";

                        if (!string.IsNullOrEmpty(filepath))
                        {
                            // Storing Image in Folder  
                            StoreInFolder(file, filepath);
                            strfiles[1] = filepath;
                        }
                       
                    }
                }               
            }
            string vmsg = "";
            int cnt = 0;
            foreach (string file in strfiles)
            {
                if (file!=null && file != "")
                {
                    cnt++;
                   await CreatePerson(model.PersonName, file, _context,vmsg);
                    if (ViewBag.Success != null)
                    {
                        vmsg = ViewBag.Success;
                    }                    
                }                
            }
            if (cnt == 0)
            {
                ViewBag.Error = "Please take a snap shot or upload an image";
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index([Bind("PersonName,CapturedFile,UploadedFile")] Face model, string browerName)
        {
             ViewBag.browerName = browerName;
            string path = ConfigurationManager.AppSetting["AppSettings:Faces"];
            string[] strfiles = new string[1];
            if (!string.IsNullOrEmpty(model.CapturedFile))
            {
                //byt = System.Text.Encoding.UTF8.GetBytes(model.CapturedFile);
                var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                var filepath = path + $@"{"Identify_"+myUniqueFileName + ".png"}";

                string base64 = model.CapturedFile.Split(',')[1];
                byte[] bytes = Convert.FromBase64String(base64);

                System.IO.File.WriteAllBytes(filepath, bytes);
                strfiles[0] = filepath;
            }
            ViewBag.imgcap = model.CapturedFile;
            string vmsg = "";
            foreach (string file in strfiles)
            {
                if (file != null && file != "")
                {
                    await IdentifyPerson(model.PersonName, file, _context, vmsg);
                    if (ViewBag.Result != null)
                    {
                        vmsg = ViewBag.Result;
                    }
                }
            }
            return View();
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
        private async Task CreatePerson(string personname, string filepath,FaceAppDBContext context,string vmsg)
        {
            Guid personid = new Guid();
            IFaceClient client = Authenticate(ENDPOINT, SUBSCRIPTION_KEY);
            byte[] buff = System.IO.File.ReadAllBytes(filepath);
            System.IO.MemoryStream ms = new System.IO.MemoryStream(buff);
            string viewbg = "";
            if (persons[0] == null)
            {
                List<PersonDetails> per = context.PersonDetails.Where(p => p.PersonName == personname).ToList();
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
                            PersonDetails personDetails = new PersonDetails();
                            personDetails.PersonId = persons[0].PersonId;
                            personDetails.PersonName = personname;
                            await context.PersonDetails.AddAsync(personDetails);
                            await context.SaveChangesAsync();
                            transaction.Commit();
                            viewbg = "Created a person";
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();
                            viewbg = "Error while creating a person";
                            return;
                        }
                    }
                }
                else
                {
                    personid = per[0].PersonId;
                    if (vmsg == "")
                    {
                        viewbg = "Person already exists so added a face to that person";
                    }
                    else
                    {
                        viewbg = vmsg;
                    }
                }
            }
            try
            {
                await WaitCallLimitPerSecondAsync();
                await Task.Delay(250);
                await client.PersonGroupPerson.AddFaceFromStreamWithHttpMessagesAsync(personGroupId, personid, ms,detectionModel:DetectionModel.Detection03);

                await client.PersonGroup.TrainAsync(personGroupId);

                // Wait until the training is completed.
                while (true)
                {
                    await Task.Delay(1000);
                    var trainingStatus = await client.PersonGroup.GetTrainingStatusAsync(personGroupId);
                    Console.WriteLine($"Training status: {trainingStatus.Status}.");
                    if (trainingStatus.Status == TrainingStatusType.Succeeded) { break; }
                }
                ViewBag.Success = viewbg;
            }
            catch (Exception e)
            {
                ViewBag.Error = "Error while adding face to a person";
                return;
            }
        }

        private async Task IdentifyPerson(string personname, string filepath, FaceAppDBContext context, string vmsg)
        {            
            IFaceClient client = Authenticate(ENDPOINT, SUBSCRIPTION_KEY);
            byte[] buff = System.IO.File.ReadAllBytes(filepath);
            System.IO.MemoryStream ms = new System.IO.MemoryStream(buff);
            string viewbg = "";

            IList<Guid> sourceFaceIds = new List<Guid>();
            // Detect faces from source image url.
            IList<DetectedFace> detectedFaces = await client.Face.DetectWithStreamAsync(ms,
                                    recognitionModel:RecognitionModel.Recognition04,detectionModel:DetectionModel.Detection03);

            // Add detected faceId to sourceFaceIds.
            foreach (var detectedFace in detectedFaces) { sourceFaceIds.Add(detectedFace.FaceId.Value); }

            // Identify the faces in a person group. 
            if (sourceFaceIds != null && sourceFaceIds.Count > 0)
            {
                var identifyResults = await client.Face.IdentifyAsync(sourceFaceIds, personGroupId);

                foreach (var identifyResult in identifyResults)
                {
                    Person person = await client.PersonGroupPerson.GetAsync(personGroupId, identifyResult.Candidates[0].PersonId);
                    /*viewbg= $"Person '{person.Name}' is identified for face in:'{identifyResult.FaceId}'," +
                        $" confidence: {identifyResult.Candidates[0].Confidence}.";*/
                    viewbg = $"Thank you {person.Name}! Welcome, it is nice to see you!";
                }
            }
            else
            {
                viewbg = $"Oops! identity not matched!, try again";
            }
            ViewBag.Result = viewbg;
        }    

        private void StoreInFolder(IFormFile file, string fileName)
        {
            using (FileStream fs = System.IO.File.Create(fileName))
            {
                file.CopyTo(fs);
                fs.Flush();
            }
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
