using Lifvs.Common.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Services.Interfaces
{
    public interface IInventoryService
    {
        List<InventoryViewModel> GetAllInventory();
        long AddInventory(InventoryAddModel model);
        long EditInventory(InventoryAddModel model);
        string DeleteInventory(long id);
        InventoryAddModel GetInventoryById(long id);
        List<InventoryViewModel> GetInventories(DateTime? currentDate);
        List<InventoryViewModel> GetStoreInventories(long storeId);
        InventoryResponseModel GetInventoryDetailById(long inventoryId);
        List<InventoryResponseModel> GetNameWiseSearchedInventories(InventorySearchModel model);
        List<InventoryDropDown> GetInventoriesDropDown();
        object GetInventoryDetailByCode(long storeId, InventorySearchModel model);
        bool IsInventoryCodeExists(string inventoryCode);
    }
}
