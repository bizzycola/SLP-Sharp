using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SwitchLanNet.Controllers
{
    [Route("/")]
    [ApiController]
    public class StatsController : Controller
    {
        SLPServer _slp;

        public StatsController(SLPServer slp)
        {
            _slp = slp;
        }

        [HttpGet]
        public IActionResult OnGet()
        {
            var data = _slp?.TestData;
            if (data == null)
                return Json(new { Success = false, Error = "Failed to retrieve server stats." });

            return Json(new
            {
                Success = true,

                SpeedStats = new
                {
                    UploadSpeed = $"{data.Upload} Bytes/s",
                    DownloadSpeed = $"{data.Download} Bytes/s"
                },

                _slp.ClientCount
            });

        }
    }
}