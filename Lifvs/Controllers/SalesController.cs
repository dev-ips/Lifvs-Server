using Lifvs.Common.ApiModels;
using Lifvs.Common.Services.Interfaces;
using log4net;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Lifvs.Controllers
{
    public class SalesController : Controller
    {
        private readonly ILog _log;
        private readonly IStoreService _storeService;
        private readonly IReceiptService _receiptService;
        public SalesController(ILog log, IStoreService storeService, IReceiptService receiptService)
        {
            _log = log;
            _storeService = storeService;
            _receiptService = receiptService;
        }
        // GET: Sales
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Stores()
        {
            return View();
        }

        public JsonResult GetStoresDropDown()
        {
            var stores = _storeService.GetStoresDropDown();
            return Json(stores, JsonRequestBehavior.AllowGet);
        }
        public ActionResult StoreReceipts(int? id)
        {
            var salesModel = new SalesModel
            {
                StoreId = id
            };
            return View(salesModel);
        }
        public JsonResult GetStoreReceipts(int storeId, int page, int rows, string sidx, string sord, bool _search = false, string filters = "")
        {
            try
            {
                var pageIndex = Convert.ToInt32(page) - 1;
                var offSet = (page * rows) - (rows - 1);
                var storeReceipts = _receiptService.GetStoreReceipts(storeId, offSet, rows, filters, sidx, sord);
                var totalRecords = storeReceipts.Count > 0 ? storeReceipts.FirstOrDefault().ResultCount : 0;
                var totalPages = (int)Math.Ceiling((float)totalRecords / (float)rows);
                var jsonData = new
                {
                    total = totalPages,
                    page = page,
                    records = totalRecords,
                    rows = storeReceipts
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Result = "Error", Message = ex.Message });
            }
        }
    }
}