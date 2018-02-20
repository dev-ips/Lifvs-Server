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
    [RoutePrefix("api/cart")]
    public class CartController : ApiController
    {
        private readonly ILog _log;
        private readonly ICartService _cartService;
        private readonly IExceptionManager _exception;

        public CartController(ILog log, ICartService cartService, IExceptionManager exceptionManager)
        {
            _log = log;
            _cartService = cartService;
            _exception = exceptionManager;
        }

        [HttpGet]
        [Route("opendoor/user/{userId}/store/{storeId}")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult OpenDoor(string userId, string storeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(storeId))
                    _exception.ThrowException(HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");

                var cartId = _cartService.GenerateCart(userId, storeId);
                return Ok(new { CartId = cartId });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in open door. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpPost]
        [Route("user/{userId}/cart/{cartId}/store/{storeId}")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult AddItemInCart(string userId, string cartId, string storeId, OfflineCartItemModel inventoryId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cartId) || inventoryId.InventoryIds.Length <= 0 || string.IsNullOrWhiteSpace(storeId))
                    throw _exception.ThrowException(HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");

                var cartItemList = _cartService.AddCartItem(userId, cartId, storeId, inventoryId);
                return Ok(new { cartItems = cartItemList });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in open door. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpPost]
        [Route("user/{userId}/cart/{cartId}/store/{storeId}/offline")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult AddOfflineItemInCartList(string userId, string cartId, string storeId, OfflineCartItemModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cartId) || string.IsNullOrWhiteSpace(storeId) || model.InventoryIds == null)
                    throw _exception.ThrowException(HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");

                var message = _cartService.AddOfflineCartItem(userId, cartId, storeId, model);
                return Ok(new { Message = message });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in open door. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpPost]
        [Route("user/{userId}/store/{storeId}/offline")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult AddOfflineItemInCartList(string userId, string storeId, OfflineCartItemModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(storeId) || model.InventoryIds == null)
                    throw _exception.ThrowException(HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");

                var cartId = _cartService.GenerateCart(userId, storeId);
                var message = _cartService.AddOfflineCartItem(userId, cartId.ToString(), storeId, model);
                return Ok(new { Message = message });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in open door. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpDelete]
        [Route("user/{userId}/store/{storeId}/inventory/{inventoryId}/cartItem/{cartItemId}")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult DeleteCartItem(string cartItemId, string storeId, string inventoryId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cartItemId) || string.IsNullOrWhiteSpace(storeId) || string.IsNullOrWhiteSpace(inventoryId))
                    throw _exception.ThrowException(HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");

                _cartService.DeleteCartItem(cartItemId, storeId, inventoryId);
                return Ok(new { Message = "Varan har blivit raderad." });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in open door. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpDelete]
        [Route("user/{userId}/store/{storeId}/cart/{cartId}/removeall")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult RemovelAllCartItems(long userId, long storeId, long cartId)
        {
            try
            {
                _cartService.RemoveAllCartItems(userId, storeId, cartId);
                return Ok(new { Message = "Varukorgen har blivit tömd." });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in removing all cart items. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpDelete]
        [Route("user/{userId}/store/{storeId}/cart/{cartId}/cancelshopping")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult CancelShopping(long userId, long storeId, long cartId)
        {
            try
            {
                _cartService.CancelShopping(userId, storeId, cartId);
                return Ok(new { Message = "Din köpsession är avslutad." });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in cancel shopping. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }
    }
}
