using Lifvs.Common.Services.Interfaces;
using Lifvs.Common.Utility;
using log4net;
using System;
using System.Web.Mvc;

namespace Lifvs.Areas.Customer.Controllers
{
    public class CustomerShopViewController : Controller
    {
        private readonly ILog _log;
        private readonly ICartService _cartService;
        private readonly IReceiptService _receiptService;

        public CustomerShopViewController(ILog log, ICartService cartService, IReceiptService receiptService)
        {
            _log = log;
            _cartService = cartService;
            _receiptService = receiptService;
        }

        // GET: CustomerShopView
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetAllCartItemCustomerShop()
        {
            try
            {
                var user = SessionRegistry.GetUserData();
                var storeId = Convert.ToString(Session["StoreId"]);
                var cartId = _cartService.GenerateCart(user.Id.ToString(), storeId);
                var cartItems = _cartService.GetAllCartItemList(cartId.ToString());

                return Json(new { success = true, data = cartItems }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Following error occured while fetching customer view shop purchase list. Error: {0}", ex.Message);
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddItemInCart(string inventoryCode)
        {
            try
            {
                var user = SessionRegistry.GetUserData();
                var storeId = Convert.ToString(Session["StoreId"]);
                var cartId = _cartService.GenerateCart(user.Id.ToString(), storeId);
                var cartItem = _cartService.AddCartItemForCustomerShop(user.Id.ToString(), cartId.ToString(), storeId, inventoryCode);

                return Json(new { success = true, data = cartItem }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Following error occured while storing customer view shop purchase item. Error: {0}", ex.Message);
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteCartItem(string itemId, string storeId, string inventoryId, string cartId)
        {
            try
            {
                _cartService.DeleteCartItem(itemId, storeId, inventoryId);
                var cartItems = _cartService.GetAllCartItemList(cartId);

                return Json(new { success = true, data = cartItems }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Following error occured while removing customer view shop purchase item. Error: {0}", ex.Message);
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EndShopping(string cartId)
        {
            try
            {
                var storeId = Convert.ToString(Session["StoreId"]);
                var user = SessionRegistry.GetUserData();
                var message = _receiptService.Payment(user.Id.ToString(), storeId, cartId);

                return Json(new { success = true, data = message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Following error occured while payment customer view shop purchase item. Error: {0}", ex.Message);
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}