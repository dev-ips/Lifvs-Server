using Lifvs.Common.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Lifvs.Common.Services.Interfaces
{
    public interface IStoreInventoryService
    {
        List<StoreInventoryModel> GetStoreInventory(int storeId);
        long GetAvailableUnit(string storeId, string inventoryId);
        long AddStoreInventory(AddStoreInventoryModel model);
        void DeleteStoreInventory(string id);
        string AddBulkDataFromCSVFile(HttpPostedFileBase fileUpload, int storeId);
    }
}
