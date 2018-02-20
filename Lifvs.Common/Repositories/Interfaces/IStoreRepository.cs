using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Repositories.Interfaces
{
    public interface IStoreRepository
    {
        List<StoresResponseModel> GetStores();
        List<int> GetStoreInventories(long storeId);
        List<StoreViewModel> GetNearByStores(string latitude, string longitude);
        List<StoreViewModel> GetSearchedStores(string searchedFields);

        List<StoreViewModel> GetAllStores();
        long AddStoreDetails(Stores model);
        void DeleteStoreDetails(long storeId);
        Stores GetStoreDetailById(long storeId);
        bool UpdateStoreDetails(Stores model);
        List<StoreViewModel> GetRecentlyVisitedStores();
        Stores GetStoreByQRCode(string qrCode, long storeId);
        List<StoreDropDownModel> GetStoresDropDown();
        long GetStoreCode(string userId, string storeId);
        long SaveStoreCode(string userId, string storeId, string code);
        bool UpdateStoreRating(Stores model);
        bool AddStoreRatings(StoreRatings model);
        decimal GetStoreAverageRatings(long storeId);
    }
}
