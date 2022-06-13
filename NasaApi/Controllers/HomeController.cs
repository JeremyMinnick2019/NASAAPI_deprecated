using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NasaApi.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using NasaApi.Classes;
using System.Threading.Tasks;

namespace NasaApi.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            bool? IsLoggedIn = HttpContext.Session.Get<bool?>("IsLoggedIn") ?? false;

            if (IsLoggedIn == false)
            {
                return Redirect(Url.Action("Login","Home"));
            }

            Random random = new Random();
            //int rnd1;
            //int rnd2;
            //int rnd3;
            //int rnd4;

            List<string> HomePagePics = new List<string>();

            int calls = 0;
            for (int i = 0; i < 5; i++)
            {
                if (calls == 20)
                {
                    //capping at 20 because you are limited to 1000 calls per hour
                    break;
                }

                int RandomSol = random.Next(0, 3109);// Numbers 0 - 3,108 will be selected. Curiosity is at 3,108 sols (3,193 earth days) on mars.

                //creating the url with a random sol. Side note computers cant actually create anything random there is only pseudorandom. Dont believe me? google it.
                string NASAURL = "https://api.nasa.gov/mars-photos/api/v1/rovers/curiosity/photos?sol="+ RandomSol.ToString() + "&api_key=" + HttpContext.Session.Get<string>("APIKey");
                calls++;

                //creating the client AKA thing the grabs the results from ^^^ this thing
                HttpClient client = new HttpClient();

                HttpResponseMessage resp = client.GetAsync(NASAURL).Result;

                //Gets the results a string
                string obj = resp.Content.ReadAsStringAsync().Result;

                //Makes a json object
                var jObject = JObject.Parse(obj);

                //makes a Jtoken with the info I need
                var JPhotos = jObject["photos"];

                //this bad so dont do this
                if (JPhotos == null || JPhotos.Count() == 0)
                {
                    //Subtracting 1 because I want it to go through 5 times successfully
                    i = i - 1;
                    continue;
                }

                //I dont want the same camera from each call most of the photos are basically the same
                List<string> UsedCameras = new List<string>();
                //Stops for loop at specified index in for loop
                int StopAt = 0;

                for (int x = 0; x < JPhotos.Count(); x++)
                {
                    //Checks to see if the camera was already used and if it was we dont care so continue
                    if (UsedCameras.Contains(JPhotos[x]["camera"]["name"].ToString()))
                    {
                        continue;
                    }

                    //adds the current camera to the list
                    UsedCameras.Add(JPhotos[x]["camera"]["name"].ToString());

                    //adds img_src (url to get the photo) to the list. To be used in the view
                    HomePagePics.Add(JPhotos[x]["img_src"].ToString());

                    //increases the value
                    StopAt++;

                    //this is where you can specify when you want to stop I recommend 5 because you will still have a lot of photos and it wont take ages to load the landing page.
                    //But you can enter whatever floats your metaphorical boat... or your real one I guess but changing that value wont make it float.
                    if (StopAt == 5)
                    {
                        break;
                    }
                }
            }

            //pseudorandomness!!!! I run it through 15 times to get the list nice and mixed up
            for (int i = 0; i < 15; i++)
            {
                HomePagePics = SupportFunctions.ShuffleMe(HomePagePics);
            }
            

            return View(HomePagePics);
        }

        public IActionResult Login()
        {
            return View();
        }

        public JsonResult LoginVerification(string APIKey) 
        {
            string URL = "https://api.nasa.gov/mars-photos/api/v1/rovers?api_key=" + APIKey;

            HttpClient client = new HttpClient();

            HttpResponseMessage response = client.GetAsync(URL).Result;

            if (response.IsSuccessStatusCode)
            {
                HttpContext.Session.Set<bool?>("IsLoggedIn", true);
                HttpContext.Session.Set<string>("APIKey", APIKey);
                return Json(new { response = true });
            }
            else
            {
                HttpContext.Session.Set<bool?>("IsLoggedIn", false);
                return Json(new { response = false });
            }
        }

        public JsonResult CanLogOut()
        {
            bool? IsLoggedIn = HttpContext.Session.Get<bool?>("IsLoggedIn") ?? false;

            if (IsLoggedIn == false)
            {
                return Json(new { result = false });
            }

            return Json(new { result = true });
        }

        public void LogOut()
        {
            HttpContext.Session.Clear();
        }
    }
}

//https://mars.nasa.gov/msl-raw-images/msss/01587/mcam/1587ML0080930040605036E01_DXXX.jpg
