using Lifvs.Common.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lifvs.Common.ApiModels;
using System.Data;
using log4net;
using Dapper;
using Lifvs.Common.Utility;

namespace Lifvs.Common.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly IDbConnection _db;
        private readonly ILog _log;

        public InventoryRepository(IDbConnection db, ILog log)
        {
            _db = db;
            _log = log;
        }

        public bool IsInventoryCodeExists(string inventoryCode)
        {
            var sqlQry = GetInventoryFromBarcodeQry(inventoryCode);
            var count = _db.ExecuteScalar<int>(sqlQry);
            return count > 0 ? true : false;
        }

        public List<InventoryViewModel> GetAllInventory()
        {
            var sqlQuery = GetAllInventoryQuery();
            var inventories = _db.Query<InventoryViewModel>(sqlQuery).ToList();
            return inventories;
        }
        public List<InventoryDropDown> GetInventoryDropDown()
        {
            var sqlQuery = GetAllInventoryQuery();
            var inventories = _db.Query<InventoryDropDown>(sqlQuery).ToList();
            return inventories;
        }
        private static string GetAllInventoryQuery()
        {
            var sqlQuery = new StringBuilder();

            sqlQuery.Append(@"SELECT i.Id,i.BrandName,i.Name,i.InventoryCode,i.CreatedBy,i.Supplier,i.Volume,i.VolumeType,i.REA,wu.Name as UserName
                              FROM Inventory i 
                              LEFT JOIN WebUser wu ON i.CreatedBy = wu.Id");
            return sqlQuery.ToString();
        }

        public long AddInventory(InventoryAddModel model)
        {
            try
            {
                var sqlQuery = GetAddInventoryQuery(model);

                var result = _db.ExecuteScalar<long>(sqlQuery, new
                {
                    @brandName = model.BrandName,
                    @name = model.Name,
                    @inventoryCode = model.InventoryCode,
                    @imagePath = model.ImagePath,
                    @createdBy = SessionRegistry.GetUserData().Id,
                    @createdDate = DateTime.Now,
                    @modifiedBy = SessionRegistry.GetUserData().Id,
                    @modifiedDate = DateTime.Now,
                    @volumeType = model.VolumeType,
                    @volume = model.Volume,
                    @specification = model.Specification.Replace("'","''"),
                    @inPrice = model.InPrice,
                    @outPrice = model.OutPriceIncVat,
                    @rea = model.REA,
                    @supplier = model.Supplier

                });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }

        private static string GetAddInventoryQuery(InventoryAddModel model)
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"INSERT INTO Inventory(BrandName,Name,InventoryCode,ImagePath,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate,VolumeType,Supplier,InPrice,OutPriceIncVat,Volume,Specification,REA) 
                              VALUES(@brandName,@name,@inventoryCode,@imagePath,@createdBy,@createdDate,@modifiedBy,@modifiedDate,@volumeType,@supplier,@inPrice,@outPrice,@volume,@specification,@rea); SELECT SCOPE_IDENTITY(); ");
            return sqlQuery.ToString();
        }

        public string DeleteInventory(long id)
        {
            var sqlQuery = GetInventoryByIdQuery(id);
            var imgPath = _db.Query<InventoryAddModel>(sqlQuery).FirstOrDefault().ImagePath;

            sqlQuery = "";

            sqlQuery = GetDeleteInventoryQuery(id);
            _db.Execute(sqlQuery);

            return imgPath;
        }

        private static string GetInventoryFromBarcodeQry(string barcode)
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(string.Format(@"Select Count(Id) FROM Inventory WHERE InventoryCode='{0}'", barcode));
            return sqlQuery.ToString();
        }

        private static string GetDeleteInventoryQuery(long id)
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(string.Format(@"DELETE FROM Inventory WHERE Id={0}", id));
            return sqlQuery.ToString();
        }

        public InventoryAddModel GetInventoryById(long id)
        {
            var sqlQuery = GetInventoryByIdQuery(id);
            var inventory = _db.Query<InventoryAddModel>(sqlQuery).FirstOrDefault();
            return inventory;
        }

        private static string GetInventoryByIdQuery(long id)
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(string.Format(@"SELECT i.Id,i.BrandName,i.Name,i.InventoryCode,i.CreatedBy,i.ImagePath,i.VolumeType,i.Supplier,i.InPrice,i.OutPriceIncVat,i.REA,i.Volume,i.Specification,wu.Email as UserName FROM Inventory i INNER JOIN WebUser wu ON i.CreatedBy = wu.Id WHERE i.Id = {0}", id));
            return sqlQuery.ToString();
        }

        public long EditInventory(InventoryAddModel model)
        {
            try
            {
                model.Name=model.Name.Replace("\r\n", string.Empty).Replace("'", "''").Replace(",", "");
                model.Specification = model.Specification.Replace("\r\n", string.Empty).Replace("'","''").Replace(",","");
                var sqlQuery = GetEditInventoryQuery(model);
                var result = _db.ExecuteScalar<long>(sqlQuery);
                return 1;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
        }

        private static string GetEditInventoryQuery(InventoryAddModel model)
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(string.Format(@"UPDATE Inventory SET BrandName='{0}',Name='{1}',InventoryCode='{2}',ModifiedBy={3},ModifiedDate='{4}',ImagePath='{5}',VolumeType='{6}',Supplier='{7}',InPrice='{8}',OutPriceIncVat='{9}',REA={10},Volume='{11}',Specification='{12}' WHERE Id={13} ", model.BrandName, model.Name, model.InventoryCode, SessionRegistry.GetUserData().Id, DateTime.Now, model.ImagePath, model.VolumeType, model.Supplier, model.InPrice, model.OutPriceIncVat, model.REA, model.Volume, model.Specification, model.Id));
            return sqlQuery.ToString();
        }
        public List<InventoryViewModel> GetInventories(DateTime? currentDate)
        {
            var inventories = new List<InventoryViewModel>();
            var sqlQuery = GetInventoriesQuery();
            if (currentDate.HasValue)
            {
                sqlQuery += " WHERE ModifiedDate >= @currentDate;";
                inventories = _db.Query<InventoryViewModel>(sqlQuery, new
                {
                    @currentDate = currentDate
                }).ToList();
            }
            else
            {
                inventories = _db.Query<InventoryViewModel>(sqlQuery).ToList();
            }

            return inventories;
        }
        private static string GetInventoriesQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM Inventory");
            return sqlQuery.ToString();
        }
        public List<InventoryViewModel> GetStoreInventories(long storeId)
        {
            var sqlQuery = GetStoreInventoriesQuery();
            var storeInventories = _db.Query<InventoryViewModel>(sqlQuery, new
            {
                @storeId = storeId
            }).ToList();
            return storeInventories;
        }
        private static string GetStoreInventoriesQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM Inventory WHERE StoreId=@storeId;");
            return sqlQuery.ToString();
        }
        public InventoryViewModel GetInventoryDetailById(long inventoryId)
        {
            var sqlQuery = GetInventoryDetailByIdQuery();
            var inventoryDetail = _db.Query<InventoryViewModel>(sqlQuery, new
            {
                @inventoryId = inventoryId
            }).FirstOrDefault();
            return inventoryDetail;
        }
        private static string GetInventoryDetailByIdQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM Inventory WHERE Id=@inventoryId;");
            return sqlQuery.ToString();
        }
        public List<InventoryViewModel> GetNameWiseSearchedInventories(InventorySearchModel model)
        {
            var sqlQuery = GetNameWiseSearchedInventoriesQuery(model.SearchKeyWord);
            var searchedInventories = _db.Query<InventoryViewModel>(sqlQuery).ToList();
            return searchedInventories;
        }
        private static string GetNameWiseSearchedInventoriesQuery(string searchKeyWord)
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(string.Format(@"SELECT * FROM Inventory WHERE Name LIKE '%{0}%'", searchKeyWord));
            return sqlQuery.ToString();
        }
        public InventoryViewModel GetInventoryDetailByCode(string qrCode)
        {
            var sqlQuery = GetInventoryDetailByCodeQuery();
            var inventoryDetail = _db.Query<InventoryViewModel>(sqlQuery, new
            {
                @qrCode = qrCode
            }).FirstOrDefault();
            return inventoryDetail;
        }
        private static string GetInventoryDetailByCodeQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM Inventory WHERE InventoryCode=@qrCode");
            return sqlQuery.ToString();
        }
        public bool IsInventoryExistInStore(long inventoryId, long storeId)
        {
            var sqlQuery = IsInventoryExistInStoreQuery();
            var isInventoryExistInStore = _db.ExecuteScalar<int>(sqlQuery, new
            {
                @storeId = storeId,
                @inventoryId = inventoryId
            });
            return isInventoryExistInStore > 0;
        }
        private static string IsInventoryExistInStoreQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT COUNT(Id) FROM StoreInventory WHERE StoreId=@storeId AND InventoryId=@inventoryId;");
            return sqlQuery.ToString();
        }
    }
}
