using Lifvs.Common.ApiModels;
using Lifvs.Common.Filters;
using Lifvs.Common.Helpers;
using Lifvs.Common.Services.Interfaces;
using log4net;
using System;
using System.Web.Http;
using System.Web.Http.Cors;
using CC = Lifvs.Common.Helpers.CommonConstants;
namespace Lifvs.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/users")]
    public class UserController : ApiController
    {
        private readonly ILog _log;
        private readonly IExceptionManager _exception;
        private readonly IUserService _userService;
        public UserController(ILog log, IExceptionManager exception, IUserService userService)
        {
            _log = log;
            _exception = exception;
            _userService = userService;
        }

        [HttpPost]
        [Route("{userId}/carddetails")]
        public IHttpActionResult AddUserCardDetais(long userId, UserCardDetailModel model)
        {
            try
            {
                model.UserId = userId;
                var userCardDetailId = _userService.AddUserCardDetails(model);
                return Ok(new { Message = "Kreditkortsuppgifter är uppdaterad." });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in adding user card details. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpGet]
        [Route("{userCardDetailId}/usercarddetails")]
        public IHttpActionResult GetUserCardDetails(long userCardDetailId)
        {
            try
            {
                var userCardDetails = _userService.GetUserCardDetails(userCardDetailId);
                return Ok(new { UserCardDetails = userCardDetails });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in getting user card details. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpGet]
        [Route("{userId}/hasvalidcard")]
        public IHttpActionResult CheckForValidCard(long userId)
        {
            try
            {
                var validCardResponse = _userService.IsValidCard(userId);
                return Ok(new { data = validCardResponse });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in verifying user card. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpGet]
        [Route("{userId}/profile")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult GetUserProfile(long userId)
        {
            try
            {
                var userProfile = _userService.GetUserProfile(userId);
                return Ok(new { data = userProfile });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in getting user profile. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }
        [HttpPut]
        [Route("{userId}/profile/change")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult ChangeProfile(long userId, UserProfileViewModel model)
        {
            try
            {
                var isProfileUpdated = _userService.ChangeProfile(userId, model);
                return Ok(new { Message = "Profilen är uppdaterad." });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in updating user profile. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }
    }
}
