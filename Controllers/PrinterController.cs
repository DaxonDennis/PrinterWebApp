using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrinterWebApp.Controllers
{
    public class PrinterController : Controller
    {
        // GET: Printer
        public ActionResult Index()
        {
            return View();
        }
    }
}