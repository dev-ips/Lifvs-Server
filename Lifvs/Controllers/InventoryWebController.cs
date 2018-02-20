using Lifvs.Alerts;
using Lifvs.Common.ApiModels;
using Lifvs.Common.Services.Interfaces;
using Lifvs.Common.Utility.Interfaces;
using log4net;
using System;
using System.Web;
using System.Web.Mvc;

namespace Lifvs.Controllers
{
    public class InventoryWebController : Controller
    {
        private readonly ILog _log;
        private readonly IInventoryService _inventoryService;
        private readonly IStoreService _storeService;
        private readonly ISetupUser _setupUser;

        // GET: InventoryWeb
        public InventoryWebController(ILog log, IInventoryService inventoryService, ISetupUser setupUser, IStoreService storeService)
        {
            _log = log;
            _inventoryService = inventoryService;
            _setupUser = setupUser;
            _storeService = storeService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetAllInventory(int page, int rows, bool _search = false)
        {
            try
            {
                var pageIndex = Convert.ToInt32(page) - 1;
                var offSet = (page * rows) - (rows - 1);
                var inventoryList = _inventoryService.GetAllInventory();
                var totalRecordCount = inventoryList.Count > 0 ? inventoryList.Count : 0;
                var totalPages = (int)Math.Ceiling((float)totalRecordCount / (float)rows);
                var jsonData = new
                {
                    total = totalPages,
                    page = page,
                    records = totalRecordCount,
                    rows = inventoryList
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = "Error", Result = ex.Message });
            }
        }

        public ActionResult AddInventory()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddInventory(InventoryAddModel model, HttpPostedFileBase imagePath)
        {
            if (ModelState.IsValid)
            {
                if (!_inventoryService.IsInventoryCodeExists(model.InventoryCode))
                {
                    model.ImagePath = (string.IsNullOrEmpty(Convert.ToString(imagePath))) ? "" : string.Format("~/InventoryUpload/{0}-{1}", Guid.NewGuid(), imagePath.FileName);

                    var result = _inventoryService.AddInventory(model);
                    if (result > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(model.ImagePath))
                            imagePath.SaveAs(Server.MapPath(model.ImagePath));

                        return RedirectToAction("Index", "InventoryWeb").WithSuccess("Inventarielista är tillagd.");
                    }
                    else
                        return View().WithError("Ett fel har uppstått vid tillagd inventarielista.");
                }
                else
                {
                    return View().WithError("Inventariekod är redan tillgänglig för en existerande produkt.");
                }
            }
            return View(model);
        }

        public JsonResult GetAllStore()
        {
            var stores = _storeService.GetAllStores();
            return Json(new { stores }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteInventory(int id)
        {
            var imgPath = _inventoryService.DeleteInventory(id);
            if (System.IO.File.Exists(Server.MapPath(imgPath)))
                System.IO.File.Delete(Server.MapPath(imgPath));

            return RedirectToAction("Index", "InventoryWeb");
        }

        public ActionResult EditInventory(long id)
        {
            var inventory = _inventoryService.GetInventoryById(id);
            return View(inventory);
        }

        [HttpPost]
        public ActionResult EditInventory(InventoryAddModel model, HttpPostedFileBase imagePath)
        {

            if (ModelState.IsValid)
            {
                model.ImagePath = (string.IsNullOrEmpty(Convert.ToString(imagePath))) ? model.FileUrl : string.Format("~/InventoryUpload/{0}-{1}", Guid.NewGuid(), imagePath.FileName);
                var result = _inventoryService.EditInventory(model);
                if (result > 0)
                {


                    if (model.ImagePath != model.FileUrl)
                    {
                        imagePath.SaveAs(Server.MapPath(model.ImagePath));
                        if (System.IO.File.Exists(model.FileUrl))
                        {
                            System.IO.File.Delete(model.FileUrl);
                        }
                    }


                    return RedirectToAction("Index", "InventoryWeb").WithSuccess("Inventarielistan har blivit uppdaterad.");
                }
                else
                    return View().WithError("Error en els detalls de l'inventari d'actualització.");

            }
            else
            {
                model.ImagePath = model.FileUrl;

            }
            return View(model);
        }
    }
}