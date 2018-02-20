using Lifvs.Common.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Services.Interfaces
{
    public interface IStoreService
    {
        List<StoresResponseModel> GetStores();
        List<StoreViewModel> GetNearByStores(StoreInputModel model);
        List<StoreViewModel> GetSearchedStores(StoreInputModel model);

        List<StoreViewModel> GetAllStores();
        Task<long> AddStoreDetails(AddStoreModel model);
        void DeleteStoreDetails(long storeId);
        AddStoreModel GetStoreDetailById(long storeId);
        Task<bool> UpdateStoreDetails(AddStoreModel model);
        List<StoreViewModel> GetRecentlyVisitedStores();
        ScanStoreResponseModel ScanStoreCode(ScanStoreModel model);
        List<StoreDropDownModel> GetStoresDropDown();
        long GetStoreCode(string userId, string storeId);
        bool CreateAndSaveStoreCode(string qrCode, string storeName);
        bool RateStore(StoreRateModel model);
    }
}
