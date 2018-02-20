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
    [RoutePrefix("api/inventory")]
    public class InventoryController : ApiController
    {
        private readonly ILog _log;
        private readonly IInventoryService _inventoryService;
        private readonly IExceptionManager _exception;
        public InventoryController(ILog log, IInventoryService inventoryService, IExceptionManager exception)
        {
            _log = log;
            _inventoryService = inventoryService;
            _exception = exception;
        }
        [HttpPost]
        [Route("inventories")]
        public IHttpActionResult GetInventories(InventorySearchModel model)
        {
            try
            {
                var inventories = _inventoryService.GetInventories(model.CurrentDate);
                return Ok(new { data = inventories });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in get all inventories. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }
        //[HttpGet]
        //[Route("store/{storeId}/inventories")]
        //public IHttpActionResult GetStoreInventories(long storeId)
        //{
        //    try
        //    {
        //        var storeInventories = _inventoryService.GetStoreInventories(storeId);
        //        return Ok(new { data = storeInventories });
        //    }
        //    catch(Exception ex)
        //    {
        //        _log.ErrorFormat("Error in getting store inventories. Error : {0}", ex.Message);
        //        _log.Error(ex);
        //        throw;
        //    }
        //}
        [HttpGet]
        [Route("{inventoryId}/inventory")]
        public IHttpActionResult GetInventory(long inventoryId)
        {
            try
            {
                var inventoryById = _inventoryService.GetInventoryDetailById(inventoryId);
                return Ok(new { data = inventoryById });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in getting inventory detail. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }
        [HttpPost]
        [Route("search")]
        public IHttpActionResult GetNameWiseSearchedInventories(InventorySearchModel model)
        {
            try
            {
                if (model == null)
                    throw _exception.ThrowException(HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");
                if (string.IsNullOrEmpty(model.SearchKeyWord))
                    throw _exception.ThrowException(HttpStatusCode.BadRequest, "", "Invalid keyword.");

                var searchedInventories = _inventoryService.GetNameWiseSearchedInventories(model);
                return Ok(new { data = searchedInventories });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in getting searched wise inventories by name. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }
        [HttpPost]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        [Route("user/{userId}/store/{storeId}/detail")]
        public IHttpActionResult GetInventoryDetailByCode(long userId,long storeId,InventorySearchModel model)
        {
            try
            {
                var inventoryDetailByCode = _inventoryService.GetInventoryDetailByCode(storeId, model);
                return Ok(new { data = inventoryDetailByCode });
            }
            catch(Exception ex)
            {
                _log.ErrorFormat("Error in getting inventory detail by code. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }
    }
}
