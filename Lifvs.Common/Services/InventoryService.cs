using Lifvs.Common.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lifvs.Common.ApiModels;
using log4net;
using Lifvs.Common.Helpers;
using Lifvs.Common.Repositories.Interfaces;
using System.Web;
using System.Configuration;

namespace Lifvs.Common.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ILog _log;
        private readonly IExceptionManager _exception;

        public InventoryService(IInventoryRepository inventoryRepository, ILog log, IExceptionManager exception)
        {
            _inventoryRepository = inventoryRepository;
            _log = log;
            _exception = exception;
        }

        public bool IsInventoryCodeExists(string inventoryCode)
        {
            return _inventoryRepository.IsInventoryCodeExists(inventoryCode);
        }

        public long AddInventory(InventoryAddModel model)
        {
            return _inventoryRepository.AddInventory(model);
        }

        public string DeleteInventory(long id)
        {
            return _inventoryRepository.DeleteInventory(id);
        }

        public long EditInventory(InventoryAddModel model)
        {
            return _inventoryRepository.EditInventory(model);
        }

        public List<InventoryViewModel> GetAllInventory()
        {
            return _inventoryRepository.GetAllInventory();
        }
        public List<InventoryDropDown> GetInventoriesDropDown()
        {
            return _inventoryRepository.GetInventoryDropDown();
        }
        public InventoryAddModel GetInventoryById(long id)
        {
            return _inventoryRepository.GetInventoryById(id);
        }

        public List<InventoryViewModel> GetInventories(DateTime? currentDate)
        {
            var inventoryListModel = new List<InventoryViewModel>();
            var webUrl = ConfigurationManager.AppSettings["WebUrl"] + "/";
            var inventories = _inventoryRepository.GetInventories(currentDate);
            inventoryListModel.AddRange(inventories.Select(x => new InventoryViewModel()
            {
                Id = x.Id,
                BrandName = x.BrandName,
                Name = x.Name,
                OutPriceIncVat = x.OutPriceIncVat,
                Specification = x.Specification,
                CreatedBy = x.CreatedBy,
                ImagePath = !string.IsNullOrEmpty(x.ImagePath) ? x.ImagePath.Replace("~/", webUrl) : null,
                InventoryCode = x.InventoryCode,
                VolumeType = x.VolumeType,
                Volume = x.Volume,
                Supplier = x.Supplier,
                REA = x.REA,
                ModifiedDate = x.ModifiedDate
            }));

            return inventoryListModel;
        }
        public List<InventoryViewModel> GetStoreInventories(long storeId)
        {
            var inventoryListModel = new List<InventoryViewModel>();
            var webUrl = ConfigurationManager.AppSettings["WebUrl"] + "/";
            var storeInventories = _inventoryRepository.GetStoreInventories(storeId);
            inventoryListModel.AddRange(storeInventories.Select(x => new InventoryViewModel()
            {
                Id = x.Id,
                BrandName = x.BrandName,
                Name = x.Name,
                OutPriceIncVat = x.OutPriceIncVat,
                Specification = x.Specification,
                CreatedBy = x.CreatedBy,
                REA = x.REA,
                ImagePath = !string.IsNullOrEmpty(x.ImagePath) ? x.ImagePath.Replace("~/", webUrl) : null
            }));
            return inventoryListModel;
        }
        public InventoryResponseModel GetInventoryDetailById(long inventoryId)
        {
            var inventoryModel = new InventoryResponseModel();
            var inventoryDetail = _inventoryRepository.GetInventoryDetailById(inventoryId);
            if (inventoryDetail == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Inventarielista saknas.");

            var webUrl = ConfigurationManager.AppSettings["WebUrl"] + "/";

            inventoryModel.Id = inventoryDetail.Id;
            inventoryModel.BrandName = inventoryDetail.BrandName;
            inventoryModel.Name = inventoryDetail.Name;
            inventoryModel.Price = inventoryDetail.OutPriceIncVat;
            inventoryModel.Description = inventoryDetail.Specification;
            inventoryModel.CreatedBy = inventoryDetail.CreatedBy;
            inventoryModel.ImagePath = !string.IsNullOrEmpty(inventoryDetail.ImagePath) ? inventoryDetail.ImagePath.Replace("~/", webUrl) : null;
            inventoryModel.InventoryCode = inventoryDetail.InventoryCode;
            inventoryDetail.REA = inventoryDetail.REA;
            //inventoryModel.TerminalId = inventoryDetail.TerminalId;

            return inventoryModel;
        }
        public List<InventoryResponseModel> GetNameWiseSearchedInventories(InventorySearchModel model)
        {
            var inventoryViewModelList = new List<InventoryResponseModel>();
            var searchedInventories = _inventoryRepository.GetNameWiseSearchedInventories(model);
            var webUrl = ConfigurationManager.AppSettings["WebUrl"] + "/";
            inventoryViewModelList = searchedInventories.ConvertAll(x => new InventoryResponseModel()
            {
                Id = x.Id,
                BrandName = x.BrandName,
                Name = x.Name,
                Price = x.OutPriceIncVat,
                REA = x.REA,
                Description = x.Specification,
                CreatedBy = x.CreatedBy,
                ImagePath = !string.IsNullOrEmpty(x.ImagePath) ? x.ImagePath.Replace("~/", webUrl) : null,
                InventoryCode = x.InventoryCode,
                //TerminalId = x.TerminalId
            });

            return inventoryViewModelList;
        }
        public object GetInventoryDetailByCode(long storeId, InventorySearchModel model)
        {
            var inventoryModel = new InventoryViewModel();
            var errorResponseModel = new ErrorResponseModel();
            var webUrl = ConfigurationManager.AppSettings["WebUrl"] + "/";

            if (string.IsNullOrEmpty(model.InventoryCode))
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "QR kod kan inte vara tomt.");

            var inventoryExist = _inventoryRepository.GetInventoryDetailByCode(model.InventoryCode);
            if (inventoryExist == null)
            {
                errorResponseModel.Message = "Inventarielista saknas.";
                return errorResponseModel;
            }
                //throw _exception.ThrowException(System.Net.HttpStatusCode.OK, "", "Inventory does not exist.");

            var isInventoryExistInStore = _inventoryRepository.IsInventoryExistInStore(inventoryExist.Id, storeId);
            if (!isInventoryExistInStore)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Det finns ingen inventarielista för denna butik.");

            inventoryModel.Id = inventoryExist.Id;
            inventoryModel.BrandName = inventoryExist.BrandName;
            inventoryModel.Name = inventoryExist.Name;
            inventoryModel.OutPriceIncVat = inventoryExist.OutPriceIncVat;
            inventoryModel.REA = inventoryExist.REA;
            inventoryModel.Specification = inventoryExist.Specification;
            inventoryModel.CreatedBy = inventoryExist.CreatedBy;
            inventoryModel.ImagePath = !string.IsNullOrEmpty(inventoryExist.ImagePath) ? inventoryExist.ImagePath.Replace("~/", webUrl) : null;
            inventoryModel.InventoryCode = inventoryExist.InventoryCode;
            inventoryModel.VolumeType = inventoryExist.VolumeType;
            inventoryModel.Volume = inventoryExist.Volume;
            inventoryModel.Supplier = inventoryExist.Supplier;
            //TerminalId = x.TerminalId,
            inventoryModel.ModifiedDate = inventoryExist.ModifiedDate;
            return inventoryModel;
        }
    }
}
