using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NasaApi.Classes;
using NasaApi.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NasaApi.Controllers
{
    public class AsteroidsController : Controller
    {
        public IActionResult Index()
        {

            bool? IsLoggedIn = HttpContext.Session.Get<bool?>("IsLoggedIn") ?? false;

            if (IsLoggedIn == false)
            {
                return Redirect(Url.Action("Login", "Home"));
            }

            return View();
        }

        public IActionResult GetAsteroids(string StartDateString, string EndDateString)
        {

            bool? IsLoggedIn = HttpContext.Session.Get<bool?>("IsLoggedIn") ?? false;

            if (IsLoggedIn == false)
            {
                return Redirect(Url.Action("Login", "Home"));
            }

            DateTime CurrentDate = DateTime.Parse(StartDateString);
            DateTime EndDate = DateTime.Parse(EndDateString);

            List<NEOInfo> NEOInfoList = new List<NEOInfo>();

            string NASAURL = "https://api.nasa.gov/neo/rest/v1/feed?start_date="+ CurrentDate.Date.ToString("yyyy-MM-dd") + "&end_date="+ EndDate.Date.ToString("yyyy-MM-dd") + "&api_key=" + HttpContext.Session.Get<string>("APIKey");

            //creating the client AKA thing the grabs the results from ^^^ this thing
            HttpClient client = new HttpClient();

            HttpResponseMessage resp = client.GetAsync(NASAURL).Result;

            //Gets the results a string
            string obj = resp.Content.ReadAsStringAsync().Result;

            //Makes a json object
            var jObject = JObject.Parse(obj);

            //makes a Jtoken with the info I need
            var JNEOOuter = jObject["near_earth_objects"];

            if (JNEOOuter == null)
            {
                return ViewComponent("NEOGrid", new { NEOInfoList = new List<NEOInfo>() });
            }

            //Its ordered by date and the date contains the info
            for (int i = 0; i < JNEOOuter.Count(); i++)
            {
                //Will move through the dates till the end
                CurrentDate = CurrentDate.Date.AddDays(i);

                // var with the current data I nedd
                string date = CurrentDate.Date.ToString("yyyy-MM-dd");
                var JNEO = JNEOOuter[date];

                if (JNEO == null)
                {
                    continue;
                }

                //for loop to add to the list
                for (int x = 0; x < JNEO.Count(); x++)
                {
                    //making the list
                    NEOInfoList.Add(new NEOInfo
                    {
                        Id = JNEO[x]["id"].ToString(),
                        NeoReferenceId = JNEO[x]["neo_reference_id"].ToString(),
                        Name = JNEO[x]["name"].ToString(),
                        NasaJPLUrl = JNEO[x]["nasa_jpl_url"].ToString(),
                        EstimatedDiameterInMetersMin = JNEO[x]["estimated_diameter"]["meters"]["estimated_diameter_min"].ToString(),
                        EstimatedDiameterInMetersMax = JNEO[x]["estimated_diameter"]["meters"]["estimated_diameter_max"].ToString(),
                        IsPotentiallyHazardousAsteroid = JNEO[x]["is_potentially_hazardous_asteroid"].ToString().Trim() == "true" ? "Yes" : "No",
                        CloseApproachDate = JNEO[x]["close_approach_data"][0]["close_approach_date_full"].ToString(),
                        MilesPerHour = JNEO[x]["close_approach_data"][0]["relative_velocity"]["miles_per_hour"].ToString(),
                        MissDistanceInMiles = JNEO[x]["close_approach_data"][0]["miss_distance"]["miles"].ToString(),
                        OrbitingBody = JNEO[x]["close_approach_data"][0]["orbiting_body"].ToString(),
                        IsSentryObject = JNEO[x]["is_sentry_object"].ToString().Trim() == "true" ? "Yes" : "No"
                    });
                }
            }

            return ViewComponent("NEOGrid", new { NEOInfoList = NEOInfoList } );
        }


    }
}
