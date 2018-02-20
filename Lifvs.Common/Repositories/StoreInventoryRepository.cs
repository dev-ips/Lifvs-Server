using Dapper;
using Lifvs.Common.ApiModels;
using Lifvs.Common.Repositories.Interfaces;
using Lifvs.Common.Utility.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Repositories
{
    public class StoreInventoryRepository : IStoreInventoryRepository
    {
        private readonly ILog _log;
        private readonly IDbConnection _db;
        private readonly ISetupUser _setupUser;

        public StoreInventoryRepository(ILog log, IDbConnection db, ISetupUser setupUser)
        {
            _log = log;
            _db = db;
            _setupUser = setupUser;
        }

        public List<StoreInventoryModel> GetStoreInventory(int storeId)
        {
            var sqlQuery = GetStoreInventoryQuery();
            var model = _db.Query<StoreInventoryModel>(sqlQuery, new
            {
                @storeId = storeId
            }).ToList();
            return model;
        }

        public long GetAvailableUnit(string storeId, string inventoryId)
        {
            var sqlQuery = GetAvailableUnitsQuery(storeId, inventoryId);
            var units = _db.ExecuteScalar<long>(sqlQuery);
            return units;
        }

        public long AddStoreInventory(AddStoreInventoryModel model)
        {
            var sqlQuery = GetStoreInventoryExitsQuery(model.StoreId.ToString(), model.InventoryId.ToString());
            var numberOfUnits = GetAvailableUnit(model.StoreId.ToString(), model.InventoryId.ToString());
            var count = _db.ExecuteScalar<long>(sqlQuery);
            if (count > 0)
            {
                sqlQuery = "";
                sqlQuery = GetEditStoreInventoryQuery();

                _db.ExecuteScalar<long>(sqlQuery, new
                {
                    @numberOfItems = numberOfUnits + model.NumberOfItems,
                    @modifiedBy = _setupUser.GetUserData().Id,
                    @modifiedDate = DateTime.Now,
                    @storeId = model.StoreId.ToString(),
                    @inventoryId = model.InventoryId.ToString()
                });
                return 1;
            }
            else
            {
                sqlQuery = "";
                sqlQuery = GetAddStoreInventoryQuery();

                var result = _db.ExecuteScalar<long>(sqlQuery, new
                {
                    @storeId = model.StoreId.ToString(),
                    @inventoryId = model.InventoryId.ToString(),
                    @numberOfItems = model.NumberOfItems,
                    @createdBy = _setupUser.GetUserData().Id,
                    @createdDate = DateTime.Now,
                    @modifiedBy = _setupUser.GetUserData().Id,
                    @modifiedDate = DateTime.Now
                });
                return result;
            }
        }

        public void DeleteStoreInventory(string id)
        {
            var sqlQuery = GetDeleteStoreInventoryQuery(id);
            _db.Query(sqlQuery);
        }

        private static string GetStoreInventoryExitsQuery(string storeId, string inventoryId)
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(string.Format(@"SELECT COUNT(*) FROM StoreInventory WHERE StoreId = {0} and InventoryId = {1} ", storeId, inventoryId));
            return sqlQuery.ToString();
        }

        private static string GetStoreInventoryQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT si.Id,si.StoreId,si.InventoryId,si.NumberOfItems,s.Name AS Store,i.Name AS Inventory 
                              FROM StoreInventory si 
                              INNER JOIN Store s ON s.Id = si.StoreId 
                              INNER JOIN Inventory i ON i.Id= si.InventoryId
                              WHERE StoreId=@storeId");
            return sqlQuery.ToString();
        }

        private static string GetAvailableUnitsQuery(string storeId, string inventoryId)
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(string.Format(@"SELECT NumberOfItems FROM StoreInventory WHERE StoreId = {0} AND InventoryId = {1}", storeId, inventoryId));
            return sqlQuery.ToString();
        }

        private static string GetAddStoreInventoryQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"INSERT INTO StoreInventory (StoreId,InventoryId,NumberOfItems,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate) VALUES 
                              (@storeId,@inventoryId,@numberOfItems,@createdBy,@createdDate,@modifiedBy,@modifiedDate); SELECT SCOPE_IDENTITY();");
            return sqlQuery.ToString();
        }

        private static string GetEditStoreInventoryQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"UPDATE StoreInventory SET NumberOfItems = @numberOfItems,ModifiedBy=@modifiedBy,ModifiedDate=@modifiedDate 
                              WHERE StoreId = @storeId AND InventoryId = @inventoryId; SELECT SCOPE_IDENTITY();");
            return sqlQuery.ToString();
        }

        private static string GetDeleteStoreInventoryQuery(string id)
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(string.Format(@"DELETE FROM StoreInventory WHERE Id = {0} ", id));
            return sqlQuery.ToString();
        }
    }
}
