using Dapper;
using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using Lifvs.Common.Repositories.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;

namespace Lifvs.Common.Repositories
{
    public class StoreRepository : IStoreRepository
    {
        private readonly IDbConnection _db;
        private readonly ILog _log;
        public StoreRepository(IDbConnection db, ILog log)
        {
            _db = db;
            _log = log;
        }

        public long GetStoreCode(string userId, string storeId)
        {
            var sqlQry = GetStoreCodeQry(userId, storeId);
            return _db.ExecuteScalar<long>(sqlQry);
        }

        public long SaveStoreCode(string userId, string storeId, string code)
        {
            var sqlQry = GenerateUserStoreCodeQry(userId, storeId, code);
            return _db.ExecuteScalar<long>(sqlQry);
        }


        public List<StoresResponseModel> GetStores()
        {
            var sqlQuery = GetStoresQuery();
            var allStores = _db.Query<StoresResponseModel>(sqlQuery).ToList();
            return allStores;
        }
        public List<int> GetStoreInventories(long storeId)
        {
            var sqlQuery = GetStoreInventoriesQuery();
            var inventoryIds = _db.Query<int>(sqlQuery, new
            {
                @storeId = storeId
            }).ToList();
            return inventoryIds;
        }
        private static string GetStoreInventoriesQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT InventoryId FROM StoreInventory WHERE StoreId=@storeId;");
            return sqlQuery.ToString();
        }
        public List<StoreViewModel> GetNearByStores(string latitude, string longitude)
        {
            var sqlQuery = GetNearByStoresQuery();
            var stores = _db.Query<StoreViewModel>(sqlQuery, new
            {
                @latitude = latitude,
                @longitude = longitude,
                @storeDistance = Convert.ToInt32(ConfigurationManager.AppSettings["StoreRadious"])
            }).ToList();
            return stores;
        }
        public List<StoreViewModel> GetSearchedStores(string searchedFields)
        {
            var sqlQuery = GetSearchedStoresQuery(searchedFields);
            var searchedStores = _db.Query<StoreViewModel>(sqlQuery).ToList();
            return searchedStores;
        }
        public bool UpdateStoreDetails(Stores model)
        {
            var sqlQuery = UpdateStoreDetailsQuery();
            _db.Execute(sqlQuery, new
            {
                @name = model.Name,
                @email = model.Email,
                @phone = string.Format("+46{0}", model.Phone),
                @address = model.Address,
                @city = model.City,
                @postalCode = model.PostalCode,
                @latitude = model.Latitude,
                @longitude = model.Longitude,
                @rating = model.Rating,
                @storeNumber = model.StoreNumber,
                @supervisorName = model.SupervisorName,
                @qrCode = model.QRCode,
                @id = model.Id
            });
            return true;
        }
        public List<StoreViewModel> GetRecentlyVisitedStores()
        {
            var sqlQuery = GetRecentlyVisitedStoresQuery();
            var recentlyVisitedStores = _db.Query<StoreViewModel>(sqlQuery).ToList();
            return recentlyVisitedStores;
        }
        private static string GetRecentlyVisitedStoresQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"WITH distinct_stores_cte AS (
	                            Select *,ROW_NUMBER() over(partition by StoreId order by ReceiptDate desc) as 'UniqueStore' from Receipt)
                                select Id,Name,Email,Phone,Address,City,PostalCode,QRCode,Latitude,Longitude,Rating,StoreNumber,SupervisorName,(SELECT COUNT(UserId) FROM StoreRatings WHERE StoreRatings.StoreId=Store.Id) as TotalRatings from Store where Id in ( Select  StoreId from distinct_stores_cte where UniqueStore = 1)");
            return sqlQuery.ToString();
        }
        private static string GetStoresQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT Id,Name,Email,Phone,Address,City,PostalCode,QRCode,Latitude,Longitude,StoreNumber,Rating,(SELECT COUNT(UserId) FROM StoreRatings WHERE StoreRatings.StoreId=Store.Id) as TotalRatings FROM Store");
            return sqlQuery.ToString();
        }
        private static string GetNearByStoresQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"DECLARE @g geography;
                              SET @g=geography::Point(@latitude,@longitude,4326);
                              SELECT Id,Name,Email,Phone,Address,City,PostalCode,QRCode,Latitude,Longitude,StoreNumber,SupervisorName,Rating,(SELECT COUNT(UserId) FROM StoreRatings WHERE StoreRatings.StoreId=Store.Id) as TotalRatings
                              ,@g.STDistance(geography::Point(Latitude,Longitude,4326)) as Distance
                              FROM Store WHERE @g.STDistance(GeoLocation) < = @storeDistance ORDER BY Distance;");
            return sqlQuery.ToString();
        }
        private static string GetSearchedStoresQuery(string searchedFields)
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(string.Format(@"SELECT Id,Name,Email,Phone,Address,City,PostalCode,QRCode,Latitude,Longitude,SupervisorName,StoreNumber,Rating,(SELECT COUNT(UserId) FROM StoreRatings WHERE StoreRatings.StoreId=Store.Id) as TotalRatings
                              FROM Store
                              WHERE Name LIKE '%{0}%'
                              OR Address LIKE '%{0}%'", searchedFields));
            return sqlQuery.ToString();
        }

        // Get All strores list
        public List<StoreViewModel> GetAllStores()
        {
            var sqlQuery = GetAllStoresQuery();
            var stores = _db.Query<StoreViewModel>(sqlQuery).ToList();
            return stores;
        }

        private static string GetAllStoresQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT Id,Name,Email,Phone,Address,City,PostalCode,SupervisorName FROM Store");
            return sqlQuery.ToString();
        }

        public long AddStoreDetails(Stores model)
        {
            var sqlQuery = AddStoreDetailsQuery();
            var storeId = _db.ExecuteScalar<long>(sqlQuery, new
            {
                @name = model.Name,
                @email = model.Email,
                @phone = string.Format("+46{0}", model.Phone),
                @address = model.Address,
                @city = model.City,
                @postCode = model.PostalCode,
                @qrCode = model.QRCode,
                @latitude = model.Latitude,
                @longitude = model.Longitude,
                @rating = model.Rating,
                @storeNumber = model.StoreNumber,
                @supervisorName = model.SupervisorName,
                @createdDate = DateTime.Now

            });
            return storeId;
        }
        public Stores GetStoreDetailById(long storeId)
        {
            var sqlQuery = GetStoreDetailByIdQuery();
            var storeDetail = _db.Query<Stores>(sqlQuery, new
            {
                @storeId = storeId
            }).FirstOrDefault();

            storeDetail.Phone = storeDetail.Phone.Substring(3, storeDetail.Phone.Length - 3);
            return storeDetail;
        }
        private static string AddStoreDetailsQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"INSERT INTO Store(Name,Email,Phone,Address,City,PostalCode,QRCode,Latitude,Longitude,Rating,StoreNumber,CreatedDate,GeoLocation,SupervisorName) VALUES (@name,@email,@phone,@address,@city,@postCode,@qrCode,@latitude,@longitude,@rating,@storeNumber,@createdDate,geography::Point(@latitude,@longitude,4326),@supervisorName); SELECT SCOPE_IDENTITY();");
            return sqlQuery.ToString();
        }

        private static string GenerateUserStoreCodeQry(string userId, string storeId, string code)
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(string.Format(@"Insert into StoreUserCode values({0},{1},{2},'{3}'); SELECT SCOPE_IDENTITY(); ", userId, storeId, code, DateTime.Now));
            return sqlQuery.ToString();
        }

        private static string GetStoreCodeQry(string userId, string storeId)
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(string.Format(@"SELECT UserStoreCode From StoreUserCode WHERE UserId = {0} AND StoreId = {1} AND CAST(CreatedDate AS Date) = '{2}'", userId, storeId, DateTime.Now.Date.ToShortDateString()));
            return sqlQuery.ToString();
        }

        public void DeleteStoreDetails(long storeId)
        {
            var sqlQuery = DeleteStoreDetailsQuery(storeId);
            _db.ExecuteScalar(sqlQuery);
        }

        private static string DeleteStoreDetailsQuery(long storeId)
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"Delete from Store where id=" + storeId + "");
            return sqlQuery.ToString();
        }
        private static string GetStoreDetailByIdQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM Store WHERE Id=@storeId;");
            return sqlQuery.ToString();
        }
        private static string UpdateStoreDetailsQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"UPDATE Store SET Name=@name,Email=@email,Phone=@phone,SupervisorName=@supervisorName,Address=@address,City=@city,PostalCode=@postalCode,QRCode=@qrCode,Latitude=@latitude,Longitude=@longitude,Rating=@rating,StoreNumber=@storeNumber,GeoLocation=geography::Point(@latitude,@longitude,4326) WHERE Id=@id;");
            return sqlQuery.ToString();
        }
        public Stores GetStoreByQRCode(string qrCode, long storeId)
        {
            var sqlQuery = GetStoreByQRCodeQuery();
            var store = _db.Query<Stores>(sqlQuery, new
            {
                @qrCode = qrCode,
                @storeId = storeId
            }).FirstOrDefault();
            return store;
        }
        private static string GetStoreByQRCodeQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM Store WHERE QRCode=@qrCode AND Id=@storeId;");
            return sqlQuery.ToString();
        }
        public List<StoreDropDownModel> GetStoresDropDown()
        {
            var sqlQuery = GetStoresDropDownQuery();
            var storeDropDown = _db.Query<StoreDropDownModel>(sqlQuery).ToList();
            return storeDropDown;
        }
        private static string GetStoresDropDownQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT Id,Name FROM Store;");
            return sqlQuery.ToString();
        }
        public bool UpdateStoreRating(Stores model)
        {
            var sqlQuery = UpdateStoreRatingQuery();
            _db.Execute(sqlQuery, new
            {
                @rating = model.Rating,
                @storeId = model.Id
            });
            return true;
        }
        private static string UpdateStoreRatingQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"UPDATE Store SET Rating=@rating WHERE Id=@storeId;");
            return sqlQuery.ToString();
        }
        public bool AddStoreRatings(StoreRatings model)
        {
            var sqlQuery = AddStoreRatingsQuery();
            _db.Execute(sqlQuery, new
            {
                @storeId = model.StoreId,
                @userId = model.UserId,
                @rating = model.Rating
            });
            return true;
        }
        private static string AddStoreRatingsQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"INSERT INTO StoreRatings(StoreId,UserId,Rating)
                              VALUES(@storeId,@userId,@rating)");
            return sqlQuery.ToString();
        }
        public decimal GetStoreAverageRatings(long storeId)
        {
            var sqlQuery = GetStoreAverageRatingsQuery();
            var averageRatings = _db.ExecuteScalar<decimal>(sqlQuery, new
            {
                @storeId = storeId
            });
            return averageRatings;
        }
        private static string GetStoreAverageRatingsQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT CAST(AVG(CAST(Rating as decimal(5,2))) as decimal(5,2)) as Average FROM StoreRatings WHERE StoreId=@storeId;");
            return sqlQuery.ToString();
        }
    }
}
