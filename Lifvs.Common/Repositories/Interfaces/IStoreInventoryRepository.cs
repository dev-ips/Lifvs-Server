using Lifvs.Common.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Repositories.Interfaces
{
    public interface IStoreInventoryRepository
    {
        List<StoreInventoryModel> GetStoreInventory(int storeId);
        long GetAvailableUnit(string storeId, string inventoryId);
        long AddStoreInventory(AddStoreInventoryModel model);
        void DeleteStoreInventory(string id);
    }
}
