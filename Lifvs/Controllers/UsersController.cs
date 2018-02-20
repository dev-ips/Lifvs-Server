using Lifvs.Common.Enums;
using Lifvs.Common.Services.Interfaces;
using Lifvs.Filters;
using log4net;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Lifvs.Controllers
{
    public class UsersController : Controller
    {
        private readonly ILog _log;
        private readonly IUserService _userService;
        public UsersController(ILog log, IUserService userService)
        {
            _log = log;
            _userService = userService;
        }
        // GET: Users
        [AuthorizeWebUser((int)Enums.Roles.SuperAdmin, (int)Enums.Roles.Admin, (int)Enums.Roles.Employee)]
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetUsers(int page, int rows, bool _search = false)
        {
            try
            {
                var pageIndex = Convert.ToInt32(page) - 1;
                var offSet = (page * rows) - (rows - 1);
                var usersList = _userService.GetUsers();
                var totalRecordsCount = usersList.Count > 0 ? usersList.FirstOrDefault().ResultCount : 0;
                var totalPages = (int)Math.Ceiling((float)totalRecordsCount / (float)rows);
                var jsonData = new
                {
                    total = totalPages,
                    page = page,
                    records = totalRecordsCount,
                    rows = usersList
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = "Error", Result = ex.Message });
            }
        }
    }
}