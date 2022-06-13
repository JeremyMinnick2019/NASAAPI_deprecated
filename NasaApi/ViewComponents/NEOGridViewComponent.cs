using Microsoft.AspNetCore.Mvc;
using NasaApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NasaApi.ViewComponents
{
    public class NEOGridViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<NEOInfo> NEOInfoList)
        {
            return View(NEOInfoList);
        }
    }
}
