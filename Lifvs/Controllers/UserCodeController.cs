using Lifvs.Common.Filters;
using Lifvs.Common.Helpers;
using Lifvs.Common.Services.Interfaces;
using log4net;
using System;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;
using CC = Lifvs.Common.Helpers.CommonConstants;

namespace Lifvs.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/usercode")]
    public class UserCodeController : ApiController
    {
        private readonly ILog _log;
        private readonly IUserCodeService _userCodeService;
        private readonly IExceptionManager _exception;

        public UserCodeController(ILog log, IExceptionManager exception, IUserCodeService userCodeService)
        {
            _log = log;
            _exception = exception;
            _userCodeService = userCodeService;
        }

        [HttpGet]
        [Route("getusercode/user/{userId}/store/{storeId}")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult GetUserCode(long userId, long storeId)
        {
            try
            {
                if (userId == 0 || storeId == 0)
                    throw _exception.ThrowException(HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");

                var userCode = _userCodeService.GenerateUserCode(userId, storeId);
                return Ok(new { data = userCode });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in getting user code. Error : {0} ", ex.Message);
                _log.Error(ex);
                throw;
            }
        }
    }
}
