using Lifvs.Common.Enums;
using Lifvs.Filters;
using System.Web.Mvc;

namespace Lifvs.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Dashboard
        [AuthorizeWebUser((int)Enums.Roles.SuperAdmin, (int)Enums.Roles.Admin, (int)Enums.Roles.Employee)]
        public ActionResult Index()
        {
            return View();
        }
    }
}