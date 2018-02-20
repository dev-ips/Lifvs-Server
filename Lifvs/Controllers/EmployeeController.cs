using Lifvs.Alerts;
using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using Lifvs.Common.Enums;
using Lifvs.Common.Services.Interfaces;
using Lifvs.Common.Utility.Interfaces;
using Lifvs.Filters;
using log4net;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Lifvs.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ILog _log;
        private readonly IEmployeeService _employeeService;
        private readonly ISetupUser _setupUser;
        public EmployeeController(ILog log, IEmployeeService employeeService, ISetupUser setupUser)
        {
            _log = log;
            _employeeService = employeeService;
            _setupUser = setupUser;
        }
        // GET: Employee
        public ActionResult Index()
        {
            var successMsg = Request.QueryString["msg"];
            if (successMsg != null)
            {
                return View().WithSuccess(successMsg);
            }
            return View();
        }
        public JsonResult GetEmployees(int page, int rows, bool _search = false)
        {
            try
            {
                var pageIndex = Convert.ToInt32(page) - 1;
                var offSet = (page * rows) - (rows - 1);
                var employeeList = _employeeService.GetEmployees();
                var totalRecordCount = employeeList.Count > 0 ? employeeList.FirstOrDefault().ResultsCount : 0;
                var totalPages = (int)Math.Ceiling((float)totalRecordCount / (float)rows);
                var jsonData = new
                {
                    total = totalPages,
                    page = page,
                    records = totalRecordCount,
                    rows = employeeList
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = "Error", Result = ex.Message });
            }
        }
        public ActionResult Accept()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Accept(InvitationModel model)
        {
            try
            {
                var user = _setupUser.GetUserData();
                if (ModelState.IsValid)
                {
                    _employeeService.SendInvitation(model, user.Id);
                    return View().WithSuccess("En aktiveringslänk har sänts till din emailadress.");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in requesting employee. Error : {0}", ex.Message);
                _log.Error(ex);
                return RedirectToAction("Accept").WithError(ex.Message);
            }
        }

        public JsonResult GetRolesDropDown()
        {
            var roles = from Enums.Roles r
                        in Enum.GetValues(typeof(Enums.Roles))
                        where (int)r > 1
                        select new
                        {
                            RoleId = (int)r,
                            Role = r.ToString()
                        };
            return Json(roles, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Invitation(int id)
        {
            try
            {
                var signUpModel = new SignUpModel();
                var invitation = _employeeService.GetWebUserInvitation(id);
                if (invitation.IsInvitationAccepted)
                {
                    throw new Exception("Du har redan registrerat dig.");
                }
                else
                {
                    signUpModel.InvitationId = invitation.Id;
                    signUpModel.Email = invitation.Email;
                    signUpModel.RoleId = invitation.RoleId;
                    signUpModel.CreatedBy = invitation.CreatedBy;
                    return View(signUpModel);
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Following error occured in getting invitation. Error: {0}", ex.Message);
                return View().WithError(ex.Message);
            }
        }
        [HttpPost]
        public ActionResult Invitation(SignUpModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var webUser = new WebUser
                    {
                        Email = model.Email,
                        Password = model.Password,
                        RoleId = model.RoleId.Value,
                        CreatedBy = model.CreatedBy,
                        CreatedDate = DateTime.Now,
                        ModifiedBy = model.CreatedBy,
                        ModifiedDate = DateTime.Now
                    };
                    var webUserId = _employeeService.CreateEmployee(webUser);
                    _employeeService.UpdateWebUserInvitation(model, webUserId);
                    if (webUserId > 0)
                        return View("Thankyou");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Following error occured while creating employee. Error :{0}", ex.Message);
                _log.Error(ex.Message);
                return View().WithError(ex.Message);
            }
            return View();
        }
        [AuthorizeWebUser((int)Enums.Roles.SuperAdmin, (int)Enums.Roles.Admin)]
        [HttpPost]
        public JsonResult UpdateEmployeeRole(EmployeeViewModel model)
        {
            var user = _setupUser.GetUserData();
            try
            {
                var isSuceess = _employeeService.UpdateEmployeeRole(model, user.Id);
                return Json(isSuceess);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Following error occured while changing employee role. Error: {0}", ex.Message);
                return Json(false);
            }
        }

        [AuthorizeWebUser((int)Enums.Roles.SuperAdmin, (int)Enums.Roles.Admin)]
        public ActionResult DeleteEmployee(int id)
        {
            try
            {
                _employeeService.DeleteEmployee(id);
                return RedirectToAction("Index").WithSuccess("Den anställde är nu raderad.");
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Following error occured while removing employee. Error: {0}", ex.Message);
                return RedirectToAction("Index").WithError("Den anställde kan inte bli raderad.");
            }
        }
    }
}