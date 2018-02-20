using Lifvs.Common.ApiModels;
using System;
using System.Collections.Generic;

namespace Lifvs.Common.Repositories.Interfaces
{
    public interface IInventoryRepository
    {
        List<InventoryViewModel> GetAllInventory();
        long AddInventory(InventoryAddModel model);
        long EditInventory(InventoryAddModel model);
        string DeleteInventory(long id);
        InventoryAddModel GetInventoryById(long id);
        List<InventoryViewModel> GetInventories(DateTime? currentDate);
        List<InventoryViewModel> GetStoreInventories(long storeId);
        InventoryViewModel GetInventoryDetailById(long inventoryId);
        List<InventoryViewModel> GetNameWiseSearchedInventories(InventorySearchModel model);
        List<InventoryDropDown> GetInventoryDropDown();
        InventoryViewModel GetInventoryDetailByCode(string qrCode);
        bool IsInventoryExistInStore(long inventoryId, long storeId);
        bool IsInventoryCodeExists(string inventoryCode);
    }
}
