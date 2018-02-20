using Lifvs.Common.Helpers;
using Lifvs.Common.Repositories.Interfaces;
using Lifvs.Common.Services.Interfaces;
using log4net;
using System.Collections.Generic;
using System.Linq;
using Lifvs.Common.ApiModels;
using System.Web;
using System.IO;
using CsvHelper;

namespace Lifvs.Common.Services
{
    public class StoreInventoryService : IStoreInventoryService
    {
        private readonly ILog _log;
        private readonly IStoreInventoryRepository _storeInventoryRepository;
        private readonly IExceptionManager _exceptionManager;

        public StoreInventoryService(ILog log, IStoreInventoryRepository storeInventoryRepository, IExceptionManager exceptionManager)
        {
            _log = log;
            _storeInventoryRepository = storeInventoryRepository;
            _exceptionManager = exceptionManager;
        }

        public long AddStoreInventory(AddStoreInventoryModel model)
        {
            return _storeInventoryRepository.AddStoreInventory(model);
        }

        public void DeleteStoreInventory(string id)
        {
            _storeInventoryRepository.DeleteStoreInventory(id);
        }

        public long GetAvailableUnit(string storeId, string inventoryId)
        {
            return _storeInventoryRepository.GetAvailableUnit(storeId, inventoryId);
        }

        public List<StoreInventoryModel> GetStoreInventory(int storeId)
        {
            return _storeInventoryRepository.GetStoreInventory(storeId);
        }
        public string AddBulkDataFromCSVFile(HttpPostedFileBase fileUpload, int storeId)
        {
            string fileName = Path.GetFileName(fileUpload.FileName);

            var bulkCopy = string.Empty;
            string path = Path.Combine(HttpContext.Current.Server.MapPath("~/InventoryUpload"), fileName);
            fileUpload.SaveAs(path);
            var csvInventoryList = new List<StoreInventoryCSVModel>();
            using (var sr = new StreamReader(path))
            {
                CsvReader csvRead = new CsvReader(sr);
                csvInventoryList = csvRead.GetRecords<StoreInventoryCSVModel>().Where(x => x.StoreId == storeId).ToList();
                foreach (var item in csvInventoryList)
                {
                    var addStoreInventoryModel = new AddStoreInventoryModel()
                    {
                        StoreId = item.StoreId,
                        InventoryId = item.InventoryId,
                        NumberOfItems = item.NumberOfItems
                    };
                    _storeInventoryRepository.AddStoreInventory(addStoreInventoryModel);
                }
                sr.Close();
            }

            return fileName;
        }
    }
}
