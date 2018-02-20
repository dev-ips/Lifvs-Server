using Lifvs.Common.ApiModels;
using Lifvs.Common.Helpers;
using Lifvs.Common.Services.Interfaces;
using log4net;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Lifvs.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api")]
    public class AccessTokenController : ApiController
    {
        private readonly ILog _log;
        private readonly IExceptionManager _exception;
        private readonly IAccessTokenService _accessTokenService;
        public AccessTokenController(ILog log, IExceptionManager exception, IAccessTokenService accessTokenService)
        {
            _log = log;
            _exception = exception;
            _accessTokenService = accessTokenService;
        }
        [HttpPost]
        [HttpOptions]
        [Route("login")]
        public IHttpActionResult VerifyLogin(AudienceCredentials model)
        {
            try
            {
                var retToken = _accessTokenService.ValidateAndCreateAccessToken(model);
                return Ok(retToken);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in verfiy login. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }
        [HttpPost]
        [Route("register")]
        public HttpResponseMessage Register(RegisterModel model)
        {
            try
            {
                var output = _accessTokenService.ValidateAndCreateUser(model);
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("ERROR IN REGISTER A NEW USER. Error :{0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpDelete]
        [HttpOptions]
        [Route("logout/users/{userId}/{accessToken}")]
        public IHttpActionResult Logout(string userId, string accessToken = default(string))
        {
            _accessTokenService.LogoutUser(userId, accessToken);
            return Ok();
        }
        [HttpPost]
        [Route("recovery")]
        public IHttpActionResult Recovery(RecoveryCode model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                    throw _exception.ThrowException(HttpStatusCode.BadRequest, "", "Emailadress kan inte vara tomt.");

                var minutes = _accessTokenService.ValidateEmailAndSendCode(model.Email);
                return Ok(new { Message = "En återställningskod har sänts till din email och den är användbar i några " + minutes + " minuter." });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in recover code for password. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpPut]
        [Route("changepassword")]
        public IHttpActionResult ChangePassword(RecoveryCode model)
        {
            try
            {
                var changePassword = _accessTokenService.ChangePassword(model);
                return Ok(new { Message = "Det nya lösenordet är nu aktiverat." });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in change user password. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpGet]
        [Route("verify/{userId}")]
        public IHttpActionResult VerifyUser(string userId)
        {
            try
            {
                //var output = _accessTokenService.VerifyUser(userId);
                return Json("Success");
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in verifying user. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpGet]
        [Route("users/{userId}/resendemail")]
        public IHttpActionResult ResendEmail(long userId)
        {
            try
            {
                var resendEmail = _accessTokenService.ResendEmail(userId);
                return Ok(new { Message = "Vi har sänt dig konfirmationen igen." });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in resend email. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpPut]
        [Route("users/{userId}/changepassword")]
        public IHttpActionResult ChangeUserPassword(long userId, ChangePasswordModel model)
        {
            try
            {
                if (model == null)
                    throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");

                model.UserId = userId;
                _accessTokenService.ChangeOldPassword(model);
                return Ok(new { Message = "Lösenordet är utbytt." });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in changing old user password. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }
    }
}
