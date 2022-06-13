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
using System.IO;
using System.Text;

namespace NasaApi.Controllers
{
    public class MarsController : Controller
    {
        public IActionResult MarsRoverForm()
        {
            bool? IsLoggedIn = HttpContext.Session.Get<bool?>("IsLoggedIn") ?? false;

            if (IsLoggedIn == false)
            {
                return Redirect(Url.Action("Login", "Home"));
            }

            return View();
        }

        public IActionResult ReturnedPhotos(string Rover, string Camera, string Sol, DateTime? EarthDay, string Page, bool IsSol)
        {
            bool? IsLoggedIn = HttpContext.Session.Get<bool?>("IsLoggedIn") ?? false;

            string EarthDayString = null;

            if (Camera != null)
            {
                Camera = "&camera=" + Camera.ToLower();
            }

            if (IsSol)
            {
                Sol = "sol=" + Sol;
            }
            else
            {
                EarthDayString = "&earth_date=" + EarthDay?.ToString("yyyy-MM-dd");
            }

            if (Page != null)
            {
                Page = "&page=" + Page;
            }

            if (IsLoggedIn == false)
            {
                return Redirect(Url.Action("Login", "Home"));
            }

            string NASAURL = "https://api.nasa.gov/mars-photos/api/v1/rovers/" + Rover.ToLower() + "/photos?" + (Sol != null ? Sol : "") + (EarthDayString != null ? EarthDayString : "") + (Camera != null ? Camera : "") + (Page != null ? Page : "") + "&api_key=" + HttpContext.Session.Get<string>("APIKey");

            //creating the client AKA thing the grabs the results from ^^^ this thing
            HttpClient client = new HttpClient();

            HttpResponseMessage resp = client.GetAsync(NASAURL).Result;

            //Gets the results a string
            string obj = resp.Content.ReadAsStringAsync().Result;

            //Makes a json object
            var jObject = JObject.Parse(obj);

            //makes a Jtoken with the info I need
            var JPhotos = jObject["photos"];
            List<string> SearchedPagePicsOne = new List<string>();
            List<string> SearchedPagePicsTwo = new List<string>();
            List<string> SearchedPagePicsThree = new List<string>();
            List<string> SearchedPagePicsForth = new List<string>();
            List<string> SearchedPagePicsFifth = new List<string>();

            bool bFirstList = true;

            bool bSecondList = false;

            bool bThirdList = false;

            bool bForthList = false;

            bool bFifthList = false;

            for (int x = 0; x < JPhotos.Count(); x++)
            {
                if (bFirstList)
                {
                    bFirstList = false;
                    bSecondList = true;
                    bThirdList = false;
                    bForthList = false;
                    bFifthList = false;
                    SearchedPagePicsOne.Add(JPhotos[x]["img_src"].ToString());
                }
                else if (bSecondList)
                {
                    bFirstList = false;
                    bSecondList = false;
                    bThirdList = true;
                    bForthList = false;
                    bFifthList = false;
                    SearchedPagePicsTwo.Add(JPhotos[x]["img_src"].ToString());
                }
                else if (bThirdList)
                {
                    bFirstList = false;
                    bSecondList = false;
                    bThirdList = false;
                    bForthList = true;
                    bFifthList = false;

                    SearchedPagePicsThree.Add(JPhotos[x]["img_src"].ToString());
                }
                else if(bForthList)
                {
                    bFirstList = false;
                    bSecondList = false;
                    bThirdList = false;
                    bForthList = false;
                    bFifthList = true;

                    SearchedPagePicsForth.Add(JPhotos[x]["img_src"].ToString());
                }
                else if (bFifthList)
                {

                    bFirstList = true;
                    bSecondList = false;
                    bThirdList = false;
                    bForthList = false;
                    bFifthList = false;

                    SearchedPagePicsFifth.Add(JPhotos[x]["img_src"].ToString());

                }

                
                
            }

            List<MarsPhotoModel> Imgs = imageToByteArray(SearchedPagePicsOne, SearchedPagePicsTwo, SearchedPagePicsThree, SearchedPagePicsForth, SearchedPagePicsFifth);

            return ViewComponent("DisplayMarsPhotos", new { PhotoInfo = Imgs });
        }

        public IActionResult MarsInstructions()
        {
            bool? IsLoggedIn = HttpContext.Session.Get<bool?>("IsLoggedIn") ?? false;

            if (IsLoggedIn == false)
            {
                return Redirect(Url.Action("Login", "Home"));
            }

            return View();
        }

        public List<MarsPhotoModel> imageToByteArray(List<string> InputImagesOne, List<string> InputImagesTwo, List<string> InputImagesThree, List<string> InputImagesFour, List<string> InputImagesFive)
        {
            HttpClient client = new HttpClient();

            List<MarsPhotoModel> Imgs = new List<MarsPhotoModel>();

            for (int i = 0; i < InputImagesOne.Count() - 1; i++)
            {
                Task<byte[]> ImgToByteOne;

                Task<byte[]> ImgToByteTwo = null;

                Task<byte[]> ImgToByteThree = null;

                Task<byte[]> ImgToByteFour = null;

                Task<byte[]> ImgToByteFive = null;

                ImgToByteOne = client.GetByteArrayAsync(InputImagesOne[i]);

                if (InputImagesTwo.Count() - 1 >= i)
                {
                    ImgToByteTwo = client.GetByteArrayAsync(InputImagesTwo[i]);
                }

                if (InputImagesThree.Count() - 1 >= i)
                {
                    ImgToByteThree = client.GetByteArrayAsync(InputImagesThree[i]);
                }

                if (InputImagesFour.Count() - 1 >= i)
                {
                    ImgToByteFour = client.GetByteArrayAsync(InputImagesFour[i]);
                }

                if (InputImagesFive.Count() - 1 >= i)
                {
                    ImgToByteFive = client.GetByteArrayAsync(InputImagesFive[i]);
                }

                Task.WaitAll(ImgToByteOne, ImgToByteTwo, ImgToByteThree, ImgToByteFour, ImgToByteFive);

                Imgs.Add(new MarsPhotoModel { PhotoByteArray = ImgToByteOne.Result, PhotoLink = InputImagesOne[i] });
                Imgs.Add(new MarsPhotoModel { PhotoByteArray = ImgToByteTwo.Result, PhotoLink = InputImagesTwo[i] });
                Imgs.Add(new MarsPhotoModel { PhotoByteArray = ImgToByteThree.Result, PhotoLink = InputImagesThree[i] });
                Imgs.Add(new MarsPhotoModel { PhotoByteArray = ImgToByteFour.Result, PhotoLink = InputImagesFour[i] });
                Imgs.Add(new MarsPhotoModel { PhotoByteArray = ImgToByteFive.Result, PhotoLink = InputImagesFive[i] });
            }
            
            return Imgs;
        }


    }
}
