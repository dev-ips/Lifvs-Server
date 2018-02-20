using Lifvs.Alerts;
using Lifvs.Common.ApiModels;
using Lifvs.Common.Enums;
using Lifvs.Common.Services.Interfaces;
using Lifvs.Common.Utility.Interfaces;
using Lifvs.Filters;
using log4net;
using System;
using System.Web;
using System.Web.Mvc;

namespace Lifvs.Controllers
{
    public class StoreInventoryController : Controller
    {
        private readonly ILog _log;
        private readonly IInventoryService _inventoryService;
        private readonly IStoreService _storeService;
        private readonly IStoreInventoryService _storeInventoryService;
        private readonly ISetupUser _setupUser;

        public StoreInventoryController(ILog log, ISetupUser setupUser, IStoreInventoryService storeInventoryService, IStoreService storeService, IInventoryService inventoryService)
        {
            _log = log;
            _setupUser = setupUser;
            _storeInventoryService = storeInventoryService;
            _storeService = storeService;
            _inventoryService = inventoryService;
        }

        // GET: StoreInventory
        public ActionResult Index()
        {
            return View();
        }
        [AuthorizeWebUser((int)Enums.Roles.SuperAdmin, (int)Enums.Roles.Admin, (int)Enums.Roles.Employee)]
        public ActionResult Stores()
        {
            return View();
        }
        [AuthorizeWebUser((int)Enums.Roles.SuperAdmin, (int)Enums.Roles.Admin, (int)Enums.Roles.Employee)]
        public ActionResult StoreInventories(int? id)
        {
            var store = _storeService.GetStoreDetailById(id.HasValue ? id.Value : 0);

            var salesModel = new SalesModel
            {
                StoreId = id,
                StoreName = store != null ? store.Name : string.Empty
            };
            TempData["StoreId"] = salesModel.StoreId;
            return View(salesModel);
        }
        public JsonResult GetStoreInventory(int storeId, int page, int rows, bool _search = false)
        {
            try
            {
                var pageIndex = Convert.ToInt32(page) - 1;
                var offSet = (page * rows) - (rows - 1);
                var storeInventoryList = _storeInventoryService.GetStoreInventory(storeId);
                var totalRecordCount = storeInventoryList.Count > 0 ? storeInventoryList.Count : 0;
                var totalPages = (int)Math.Ceiling((float)totalRecordCount / (float)rows);
                var jsonData = new
                {
                    total = totalPages,
                    page = page,
                    records = totalRecordCount,
                    rows = storeInventoryList
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = "Error", Result = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase fileUpload, SalesModel model)
        {
            //var storeId = Request["StoreId"];
            var bulkCopy = string.Empty;
            if (fileUpload != null)
            {
                if (fileUpload.ContentLength > 0)
                {
                    bulkCopy = _storeInventoryService.AddBulkDataFromCSVFile(fileUpload, Convert.ToInt32(model.StoreId.Value));
                    return Redirect(Url.Action("StoreInventories", "StoreInventory") + "?id=" + model.StoreId).WithSuccess("Butiksinventarielista har blivit tillagd.");
                }
            }
            else
            {
                return Redirect(Url.Action("StoreInventories", "StoreInventory") + "?id=" + model.StoreId).WithError("Vänligen välj fil.");
            }
            return Redirect(Url.Action("StoreInventories", "StoreInventory") + "?id=" + model.StoreId);
        }

        public ActionResult AddStoreInventory()
        {
            var storeId = Convert.ToInt32(TempData["StoreId"]);
            var addStoreInventoryModel = new AddStoreInventoryModel
            {
                StoreId = storeId
            };
            TempData.Keep("StoreId");
            return View(addStoreInventoryModel);
        }

        [HttpPost]
        public ActionResult AddStoreInventory(AddStoreInventoryModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = _storeInventoryService.AddStoreInventory(model);
                    if (result > 0)
                        return RedirectToAction("AddStoreInventory").WithSuccess("Butiksinventarielista har blivit tillagd.");
                    else
                        return Redirect(Url.Action("StoreInventories", "StoreInventory") + "?id=" + model.StoreId).WithError("Fel vid sparandet av butiksinventarielista.");
                }
                else
                {
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in updating store details. Error : {0}", ex.Message);
                _log.Error(ex);
                return View().WithError(ex.Message);
            }
        }

        public ActionResult DeleteStoreInventory(string id)
        {
            try
            {
                _storeInventoryService.DeleteStoreInventory(id);
                return RedirectToAction("Index").WithSuccess("Butiksinventarielista har blivt borttagen.");
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Following error occured while removing store. Error: {0}", ex.Message);
                return RedirectToAction("Index").WithError("Butiksinventarielista kan inte tas bort.");
            }
        }

        public JsonResult FillDropDownData()
        {
            var stores = _storeService.GetAllStores();
            var inventories = _inventoryService.GetInventoriesDropDown();
            return Json(new { success = true, Stores = stores, Inventories = inventories }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAvailableUnits(string storeId, string inventoryId)
        {
            var availableUnits = _storeInventoryService.GetAvailableUnit(storeId, inventoryId);
            return Json(new { success = true, units = availableUnits }, JsonRequestBehavior.AllowGet);
        }
    }
}