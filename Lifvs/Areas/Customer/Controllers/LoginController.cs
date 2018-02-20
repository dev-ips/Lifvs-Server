using Lifvs.Alerts;
using Lifvs.Common.ApiModels;
using Lifvs.Common.Services.Interfaces;
using Lifvs.Common.Utility.Interfaces;
using log4net;
using System;
using System.Web.Mvc;
using System.Web.Security;

namespace Lifvs.Areas.Customer.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAccessTokenService _accessTokenService;
        private readonly ISetupUser _setupUser;
        private readonly ILog _log;
        private readonly IStoreService _storeService;

        public LoginController(IAccessTokenService accessTokenService, ISetupUser setupUser, ILog log, IStoreService storeService)
        {
            _accessTokenService = accessTokenService;
            _setupUser = setupUser;
            _log = log;
            _storeService = storeService;
        }


        // GET: Login
        public ActionResult Index()
        {
            if (Request.Cookies["UserName"] != null && Request.Cookies["Password"] != null)
            {
                var loginModel = new LoginModel
                {
                    Email = Request.Cookies["UserName"].Value,
                    Password = Request.Cookies["Password"].Value
                };
                return View(loginModel);
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(LoginModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = _accessTokenService.GetUser(model);

                    if (user == null)
                        return View().WithError("Invalid email address and password.");
                    _setupUser.SetupUserDetail(Convert.ToInt32(user.Id), user);

                    Session["StoreId"] = Request["Stores"].ToString();

                    if (model.RememberMe)
                    {
                        Response.Cookies["UserName"].Expires = DateTime.Now.AddDays(30);
                        Response.Cookies["Password"].Expires = DateTime.Now.AddDays(30);
                        Response.Cookies["UserName"].Value = model.Email;
                        Response.Cookies["Password"].Value = model.Password;
                    }
                    return RedirectToAction("Index", "CustomerShopView");
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Following error occured while getting web user info : {0}", ex.Message);
                _log.Error(ex.Message);
                return View(model);
            }

            return View();
        }


        public ActionResult Recovery()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Recovery(RecoveryEmail model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var minutes = _accessTokenService.ValidateEmailAndSendCode(model.Email);
                    return RedirectToAction("ChangePassword").WithSuccess("Code has been sent to your email.It will expire in " + minutes + " minutes.");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error occured during email recovery. Error : {0}", ex.Message);
                _log.Error(ex);
                return View().WithError(ex.Message);
            }
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }

        public JsonResult BindStore()
        {
            var stores = _storeService.GetAllStores();
            return Json(new { success = true, Stores = stores }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChangePassword(ChangePassword model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var recoverCode = new RecoveryCode
                    {
                        Email = model.Email,
                        NewPassword = model.Password,
                        RecoverCode = model.RecoveryCode
                    };
                    var changeWebPassword = _accessTokenService.ChangePassword(recoverCode);
                    return RedirectToAction("Index").WithSuccess("Password changed successfully.");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in changing user password. Error : {0}", ex.Message);
                _log.Error(ex);
                return View().WithError("Error in changing user password");
            }
        }
        public ActionResult ScanCode()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ScanCode(ScanStoreModel model)
        {
            try
            {
                var userByBarCode = _accessTokenService.GetUserByBarCode(model.QRCode);
                if (userByBarCode == null)
                    return View().WithError("Invalid bar code for this user.");

                _setupUser.SetupUserDetail(Convert.ToInt32(userByBarCode.Id), userByBarCode);
                return RedirectToAction("Index", "CustomerShopView");
            }
            catch(Exception ex)
            {
                _log.ErrorFormat("Error in validating user by bar code. Error : {0}", ex.Message);
                _log.Error(ex);
                return View(model);
            }
            
        }
    }
}