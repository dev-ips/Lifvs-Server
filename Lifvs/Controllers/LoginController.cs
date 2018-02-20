using Lifvs.Alerts;
using Lifvs.Common.ApiModels;
using Lifvs.Common.Services.Interfaces;
using Lifvs.Common.Utility.Interfaces;
using log4net;
using System;
using System.Web.Mvc;
using System.Web.Security;

namespace Lifvs.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAccessTokenService _accessTokenService;
        private readonly ISetupUser _setupUser;
        private readonly ILog _log;
        public LoginController(IAccessTokenService accessTokenService, ISetupUser setupUser, ILog log)
        {
            _accessTokenService = accessTokenService;
            _setupUser = setupUser;
            _log = log;
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
                    var sessionUser = _accessTokenService.GetWebUser(model);
                    if (sessionUser == null)
                        return View().WithError("Nom d'usuari i contrasenya no vàlids.");
                    _setupUser.SetupUserDetail(Convert.ToInt32(sessionUser.Id), sessionUser);
                    if (model.RememberMe)
                    {
                        Response.Cookies["UserName"].Expires = DateTime.Now.AddDays(30);
                        Response.Cookies["Password"].Expires = DateTime.Now.AddDays(30);
                        Response.Cookies["UserName"].Value = model.Email;
                        Response.Cookies["Password"].Value = model.Password;
                    }
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Following error occured while getting web user info : {0}", ex.Message);
                _log.Error(ex.Message);
                return View(model);
            }
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
                    var minutes = _accessTokenService.ValidateWebEmailAndSendCode(model.Email);
                    return RedirectToAction("ChangePassword").WithSuccess("En kod har blivit sänd till din emailadress och den är giltig i några " + minutes + " minuter.");
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
                    var changeWebPassword = _accessTokenService.ChangeWebPassword(recoverCode);
                    return RedirectToAction("Index").WithSuccess("Lösenordet har blivit utbytt.");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in changing web user password. Error : {0}", ex.Message);
                _log.Error(ex);
                return View().WithError(ex.Message);
            }
        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }
    }
}