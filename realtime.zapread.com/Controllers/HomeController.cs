﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace realtime.zapread.com.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        [Route("home/index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}