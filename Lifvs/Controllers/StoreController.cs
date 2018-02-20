using Lifvs.Common.ApiModels;
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
    [RoutePrefix("api/stores")]
    public class StoreController : ApiController
    {
        private readonly ILog _log;
        private readonly IStoreService _storeService;
        private readonly IExceptionManager _exception;
        public StoreController(ILog log, IStoreService storeService, IExceptionManager exception)
        {
            _log = log;
            _storeService = storeService;
            _exception = exception;
        }

        [HttpGet]
        [Route("allstores")]
        public IHttpActionResult GetAllStores()
        {
            try
            {
                var allStores = _storeService.GetStores();
                return Ok(new { data = allStores });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in getting all stores. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpPost]
        [Route("nearbystores")]
        public IHttpActionResult GetNearByStores(StoreInputModel model)
        {
            try
            {
                if (model == null)
                    throw _exception.ThrowException(HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");
                var nearByStores = _storeService.GetNearByStores(model);
                return Ok(new { data = nearByStores });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in getting near by stores. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }
        [HttpPost]
        [Route("searchstores")]
        public IHttpActionResult SearchStores(StoreInputModel model)
        {
            try
            {
                var searchedStores = _storeService.GetSearchedStores(model);
                return Ok(new { data = searchedStores });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in searching stores by field. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }
        [HttpGet]
        [Route("{userId}/recentlyvisited")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult GetRecentlyVisitedStores(long userId)
        {
            try
            {
                var recentlyVisitedStores = _storeService.GetRecentlyVisitedStores();
                return Ok(new { data = recentlyVisitedStores });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in getting recently visited stores. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpPost]
        [Route("{userId}/store/{storeId}/scancode")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult ScanStoreCode(long userId, long storeId, ScanStoreModel model)
        {
            try
            {
                model.UserId = userId;
                model.StoreId = storeId;
                if (string.IsNullOrEmpty(model.QRCode))
                    throw _exception.ThrowException(HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");
                var scanStoreResponse = _storeService.ScanStoreCode(model);
                return Ok(new { data = scanStoreResponse });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in scanning store code. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpGet]
        [Route("user/{userId}/store/{storeId}/storecode")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult GetStoreCode(string userId, string storeId)
        {
            try
            {
                if (string.IsNullOrEmpty(storeId))
                    throw _exception.ThrowException(HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");
                var storeCode = _storeService.GetStoreCode(userId, storeId);
                return Ok(new { storeCode = storeCode });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in scanning store code. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }
        [HttpPost]
        [Route("user/{userId}/store/{storeId}/rate")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult RateStore(long userId, long storeId, StoreRateModel model)
        {
            try
            {
                model.UserId = userId;
                model.StoreId = storeId;
                _storeService.RateStore(model);
                return Ok(new { Message = "butik betygsatt framgångsrikt." });
            }
            catch(Exception ex)
            {
                _log.ErrorFormat("Error in rating store. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }
    }
}
