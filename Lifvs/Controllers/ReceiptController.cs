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
    [RoutePrefix("api/receipt")]
    public class ReceiptController : ApiController
    {
        private readonly ILog _log;
        private readonly IReceiptService _receiptService;
        private readonly IExceptionManager _exception;

        public ReceiptController(ILog log, IExceptionManager exception, IReceiptService receiptService)
        {
            _log = log;
            _exception = exception;
            _receiptService = receiptService;
        }

        [HttpPost]
        [Route("payment/user/{userId}/store/{storeId}/cart/{cartId}")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult PaymentRequest(string userId, string storeId, string cartId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(storeId) || string.IsNullOrWhiteSpace(cartId))
                    _exception.ThrowException(HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");

                var message = _receiptService.Payment(userId, storeId, cartId);
                return Ok(new { Message = message });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in open door. Error : {0}", ex.Message);
                _log.Error(ex);
                throw _exception.ThrowException(HttpStatusCode.BadRequest, "", ex.Message);
            }
        }
        [HttpGet]
        [Route("{receiptId}/user/{userId}/historydetail")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult GetReceiptHistoryDetail(long receiptId, long userId)
        {
            try
            {
                var receiptHistoryDetails = _receiptService.GetReceiptHistoryDetail(userId, receiptId);
                return Ok(new { data = receiptHistoryDetails });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in getting history receipt detail. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

        [HttpGet]
        [Route("user/{userId}/purchase")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult PurchaseHistory(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    _exception.ThrowException(HttpStatusCode.BadRequest, "", "Ogiltig förfrågan.");

                var purchasedListHistory = _receiptService.GetPurchasedHistory(userId);
                return Ok(new { PurchaseHistory = purchasedListHistory });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in fetching purchase history. Error : {0}", ex.Message);
                _log.Error(ex);
                throw _exception.ThrowException(HttpStatusCode.BadRequest, "", ex.Message);
            }
        }

        [HttpPost]
        [Route("{receiptId}/user/{userId}/sendreceipt")]
        [AccessTokenAuthentication]
        [AuthorizeUser(Roles = CC.RoleUser)]
        public IHttpActionResult SendReceipt(long receiptId, long userId)
        {
            try
            {
                _receiptService.SendReceipt(receiptId, userId);
                return Ok(new { Message = "Ett kvitto har blivit sänt till din emailadress." });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in sending receipt in email. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }


        [HttpGet]
        [Route("expiredcarts")]
        public IHttpActionResult GetAllExpiredCardAndPaymentPending()
        {
            try
            {
                _receiptService.CheckAllExpiredCarts();
                return Ok(new { Message = "Automatisk betalning har blivit genomförd." });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in sending receipt in email. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }

    }
}
