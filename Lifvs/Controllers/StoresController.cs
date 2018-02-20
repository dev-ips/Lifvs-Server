using Lifvs.Alerts;
using Lifvs.Common.ApiModels;
using Lifvs.Common.Enums;
using Lifvs.Common.Services.Interfaces;
using Lifvs.Common.Utility.Interfaces;
using Lifvs.Filters;
using log4net;
using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Drawing.Imaging;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Lifvs.Controllers
{
    public class StoresController : Controller
    {
        private readonly ILog _log;
        private readonly IStoreService _storeService;
        private readonly ISetupUser _setupUser;
        private readonly ICryptoGraphy _cryptoGraphy;
        public StoresController(ILog log, IStoreService storeService, ISetupUser setupUser, ICryptoGraphy cryptoGraphy)
        {
            _log = log;
            _storeService = storeService;
            _setupUser = setupUser;
            _cryptoGraphy = cryptoGraphy;
        }

        // GET: Stores
        [AuthorizeWebUser((int)Enums.Roles.SuperAdmin, (int)Enums.Roles.Admin, (int)Enums.Roles.Employee)]
        public ActionResult Index()
        {
            return View();
        }


        public JsonResult GetStores(int page, int rows, bool _search = false)
        {
            try
            {
                var pageIndex = Convert.ToInt32(page) - 1;
                var offSet = (page * rows) - (rows - 1);
                var storeList = _storeService.GetAllStores();
                var totalRecordCount = storeList.Count > 0 ? storeList.Count : 0;
                var totalPages = (int)Math.Ceiling((float)totalRecordCount / (float)rows);
                var jsonData = new
                {
                    total = totalPages,
                    page = page,
                    records = totalRecordCount,
                    rows = storeList
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = "Error", Result = ex.Message });
            }
        }
        [AuthorizeWebUser((int)Enums.Roles.SuperAdmin, (int)Enums.Roles.Admin)]
        public ActionResult AddStore()
        {

            return View();
        }

        [AuthorizeWebUser((int)Enums.Roles.SuperAdmin, (int)Enums.Roles.Admin)]
        [HttpPost]
        public async Task<ActionResult> AddStore(AddStoreModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.QRCode = Guid.NewGuid() + "-" + _cryptoGraphy.GenerateCode();
                    var success = await _storeService.AddStoreDetails(model);
                    var codeCreated = _storeService.CreateAndSaveStoreCode(model.QRCode, model.Name);
                    if (success > 0)
                        return RedirectToAction("Index").WithSuccess("En butik har blivit tillgad.");
                    else
                        return View(model).WithError("Error vid sparandet av butiksdetaljer.");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in adding store details. Error : {0}", ex.Message);
                _log.Error(ex);
                return View().WithError(ex.Message);
            }

        }
        [AuthorizeWebUser((int)Enums.Roles.SuperAdmin, (int)Enums.Roles.Admin)]
        public ActionResult Edit(int? Id)
        {
            try
            {
                var storeDetail = _storeService.GetStoreDetailById(Convert.ToInt64(Id.Value));
                return View(storeDetail);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in getting store details. Error : {0}", ex.Message);
                _log.Error(ex);
                return View().WithError(ex.Message);
            }

        }

        [AuthorizeWebUser((int)Enums.Roles.SuperAdmin, (int)Enums.Roles.Admin)]
        [HttpPost]
        public async Task<ActionResult> Edit(AddStoreModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _storeService.UpdateStoreDetails(model);
                    return RedirectToAction("Index").WithSuccess("Butikens uppgifter har blivit uppdaterade.");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Error in updating store details. Error : {0}", ex.Message);
                _log.Error(ex);
                return View().WithError(ex.Message);
            }

        }
        [AuthorizeWebUser((int)Enums.Roles.SuperAdmin, (int)Enums.Roles.Admin)]
        public ActionResult DeleteStore(int id)
        {
            try
            {
                _storeService.DeleteStoreDetails(id);
                return RedirectToAction("Index").WithSuccess("Butiken är borttagen.");
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Following error occured while removing store. Error: {0}", ex.Message);
                return RedirectToAction("Index").WithError("Butiken kan inte tas bort.");
            }
        }
        [AuthorizeWebUser((int)Enums.Roles.SuperAdmin, (int)Enums.Roles.Admin)]
        public ActionResult ScanQRCode()
        {
            return View();
        }
        [AuthorizeWebUser((int)Enums.Roles.SuperAdmin, (int)Enums.Roles.Admin)]
        [HttpPost]
        public ActionResult ScanQRCode(ScanStoreModel model)
        {
            if (string.IsNullOrEmpty(model.QRCode))
                return View().WithError("Codi QR no vàlid.");
            using (MemoryStream ms = new MemoryStream())
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(model.QRCode, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                using (Bitmap bitMap = qrCode.GetGraphic(20))
                {
                    bitMap.Save(ms, ImageFormat.Png);
                    ViewBag.QRCodeImage = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                }
            }
            return View();
        }
        [AuthorizeWebUser((int)Enums.Roles.SuperAdmin, (int)Enums.Roles.Admin)]
        public ActionResult DownloadCode(int? id)
        {
            var store = _storeService.GetStoreDetailById(Convert.ToInt64(id));
            if (store == null)
                return RedirectToAction("Index").WithError("Store does not exist.");

            var fileName = store.QRCode;
            if (fileName != null)
            {
                Response.ContentType = "application/pdf";
                Response.AppendHeader("Content-Disposition", "attachment; filename=" + fileName + ".pdf");
                Response.TransmitFile(Server.MapPath("~/PdfFiles" + "\\" + fileName + ".pdf"));
                Response.End();
            }
            return RedirectToAction("Index");
        }

    }
}