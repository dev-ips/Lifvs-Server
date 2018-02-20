using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lifvs.Areas.Customer.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard

        public DashboardController()
        {

        }

        public ActionResult Index()
        {
            return View();
        }
    }
}