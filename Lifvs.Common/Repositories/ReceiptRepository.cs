using Dapper;
using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using Lifvs.Common.Helpers;
using Lifvs.Common.Repositories.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Lifvs.Common.Repositories
{
    public class ReceiptRepository : IReceiptRepository
    {
        private readonly ILog _log;
        private readonly IDbConnection _db;
        private readonly IExceptionManager _exception;

        public ReceiptRepository(ILog log, IDbConnection db, IExceptionManager exception)
        {
            _log = log;
            _db = db;
            _exception = exception;
        }

        public decimal GetCartAmount(string cartId)
        {
            var sqlQry = GetCalculateCartAmountQry(cartId);
            var cartAmount = _db.Query<decimal>(sqlQry).FirstOrDefault();
            return (cartAmount * 100);
        }

        public long AddReceiptItems(string userId, string storeId, string cartId)
        {
            var sqlQry = GetCalculateCartAmountQry(cartId);
            var cartAmount = _db.Query<decimal>(sqlQry).FirstOrDefault();
            var receiptId = 0L;
            if (cartAmount > 0)
            {
                receiptId = GenerateReceipt(userId, storeId, cartAmount); // Generate new receipt

                sqlQry = string.Empty;
                sqlQry = GetAllCartItemsQry(cartId);
                var cartItems = _db.Query<CartItemModel>(sqlQry).ToList();
                foreach (var item in cartItems)
                { // generate receipt items..

                    sqlQry = string.Empty;
                    sqlQry = GetAddReceiptItemQry();
                    _db.Query(sqlQry, new
                    {
                        @receiptId = receiptId,
                        @inventoryId = item.InventoryId,
                        @quantity = item.Quantity,
                        @amount = item.Amount,
                        @createdDate = item.CreatedDate,
                        @createdBy = item.CreatedBy
                    });
                }
            }

            sqlQry = string.Empty;
            sqlQry = GetUpdateCartPaymentFlagQry(cartAmount, cartId); // update cart payment flag
            _db.Query(sqlQry);

            return receiptId;
        }

        public string CreateTransaction(string userId, string paymentStatus, string paymentId, string paymentObject, string receiptId)
        {
            var sqlQry = GetGenerateTransactionQry(); // Generate Transaction
            var transactionId = _db.ExecuteScalar<long>(sqlQry, new
            {
                @transDate = DateTime.Now,
                @userId = userId,
                @staus = paymentStatus,
                @paymentId = paymentId,
                @paymentObject = paymentObject,
                @createdDate = DateTime.Now,
                @createdBy = userId,
                @receiptId = receiptId
            });

            if (transactionId > 0)
                return "Kvitto är genererat.";
            else
                return "Betalning är gjord och det har blivit ett fel i transaktionen.";

        }

        public long GenerateReceipt(string userId, string storeId, decimal cartAmount)
        {
            var sqlQry = GetGenerateReceiptQry();
            var receiptId = _db.ExecuteScalar<long>(sqlQry, new
            {
                @receiptDate = DateTime.Now,
                @receiptAmount = cartAmount,
                @vat1 = (cartAmount * CommonConstants.Vat1InPercentage) / 100,
                @vat2 = (cartAmount * CommonConstants.Vat2InPercentage) / 100,
                @paymentDone = true,
                @createdDate = DateTime.Now,
                @createdBy = userId,
                @userId = userId,
                @storeId = storeId
            });
            return receiptId;
        }

        public List<ExpiredCardModel> CheckAllExpiredCarts()
        {
            var sqlQry = GetAllExpiredCarts();
            var data = _db.Query<ExpiredCardModel>(sqlQry).ToList();
            return data;
        }

        public List<PurchasedHistory> GetPurchasedHistory(string userId)
        {
            var sqlQry = GetPurchaseHistoryQry(userId);
            var purchaselist = _db.Query<PurchasedHistory>(sqlQry).ToList();
            return purchaselist;
        }

        public void UpdateUserCreditCardId(string creditCardId, string userId)
        {
            var sqlQry = GetUpdateUserClientIdQry(creditCardId, userId);
            _db.Query(sqlQry);
        }

        public UserCardDetailModel GetUserCardDetails(string userId)
        {
            var sqlQry = GetUserCardDetialsQry(userId);
            var model = _db.Query<UserCardDetailModel>(sqlQry).FirstOrDefault();
            return model;
        }

        public List<ReceiptItemModel> GetAllReceiptItems(string receiptId)
        {
            var sqlQry = GetAllReceiptItemsQry(receiptId);
            var receiptItems = _db.Query<ReceiptItemModel>(sqlQry).ToList();
            return receiptItems;
        }

        private static string GetAllReceiptItemsQry(string receiptId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"SELECT r.Id,r.InventoryId,r.Amount,i.Name AS Inventory,u.Email FROM ReceiptItem r INNER JOIN Inventory i on r.InventoryId = i.Id INNER JOIN [User] u ON r.CreatedBy = u.Id WHERE ReceiptId = {0}", receiptId));
            return sqlQry.ToString();
        }

        private static string GetPurchaseHistoryQry(string userId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"SELECT r.id,r.Amount,r.StoreId,r.ReceiptDate,s.Name AS Store,(SELECT COUNT(Id) FROM ReceiptItem WHERE ReceiptId = r.Id) AS TotalPurchasedItem FROM Receipt r INNER JOIN Store s ON r.StoreId = s.Id WHERE PaymentDone=1 AND UserId = {0}", userId));
            return sqlQry.ToString();
        }

        private static string GetUpdateUserClientIdQry(string creditCardId, string userId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"UPDATE UserCardDetails SET CreditCardId = '{0}' WHERE UserId = {1}", creditCardId, userId));
            return sqlQry.ToString();
        }

        private static string GetUserCardDetialsQry(string userId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"SELECT Id,CardNumber,ExpiredMonth,ExpiredYear,CVC,CreditCardId FROM UserCardDetails WHERE UserId = {0}", userId));
            return sqlQry.ToString();
        }


        private static string GetCalculateCartAmountQry(string cartId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"SELECT ISNULL(SUM(Amount),0) AS Amount FROM CartItem WHERE CartId = {0} ", cartId));
            return sqlQry.ToString();
        }

        private static string GetAllExpiredCarts()
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"SELECT Id,CartGeneratedDate,UserId,StoreId FROM Cart WHERE PaymentDone = '{0}' ORDER BY Id ", false));
            return sqlQry.ToString();
        }

        private static string GetGenerateTransactionQry()
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(@"INSERT INTO [Transaction] (TransactionDate,UserId,Status,PaymentId,PaymentObject,CreatedDate,CreatedBy,ReceiptId) VALUES (@transDate,@userId,@staus,@paymentId,@paymentObject,@createdDate,@createdBy,@receiptId); SELECT SCOPE_IDENTITY(); ");
            return sqlQry.ToString();
        }

        private static string GetUpdateCartPaymentFlagQry(decimal cartAmount, string cartId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"UPDATE cart SET Amount = {0},Vat1 = {1},Vat2 = {2}, PaymentDone = '{3}' WHERE Id = {4}", cartAmount, (cartAmount * CommonConstants.Vat1InPercentage) / 100, (cartAmount * CommonConstants.Vat2InPercentage) / 100, true, cartId));
            return sqlQry.ToString();
        }

        private static string GetAllCartItemsQry(string cartId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"SELECT CartId,InventoryId,Quantity,Amount,CreatedDate,CreatedBy FROM CartItem WHERE CartId = {0} ", cartId));
            return sqlQry.ToString();
        }

        private static string GetGenerateReceiptQry()
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(@"INSERT INTO Receipt (ReceiptDate,Amount,Vat1,Vat2,PaymentDone,CreatedDate,CreatedBy,UserId,StoreId) values (@receiptDate, @receiptAmount,@vat1,@vat2, @paymentDone, @createdDate, @createdBy, @userId, @storeId); SELECT SCOPE_IDENTITY();");
            return sqlQry.ToString();
        }

        private static string GetInventoryPriceQry(string inventoryId)
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(string.Format(@"SELECT Price FROM Inventory WHERE Id = {0}", inventoryId));
            return sqlQry.ToString();
        }

        private static string GetAddReceiptItemQry()
        {
            var sqlQry = new StringBuilder();
            sqlQry.Append(@"INSERT INTO ReceiptItem (ReceiptId,InventoryId,Quantity,Amount,CreatedDate,CreatedBy) VALUES (@receiptId,@inventoryId,@quantity,@amount,@createdDate,@createdBy); SELECT SCOPE_IDENTITY();");
            return sqlQry.ToString();
        }
        public List<ReceiptViewModel> GetStoreReceipts(int storeId, string whereCondition, string orderBy)
        {
            var sqlQuery = GetStoreReceiptsQuery();
            sqlQuery = !string.IsNullOrEmpty(whereCondition)
                     ? sqlQuery.Replace("#WHERECONDITION#", string.Format("{0}", whereCondition))
                     : sqlQuery.Replace("#WHERECONDITION#", string.Empty);

            sqlQuery = !string.IsNullOrEmpty(orderBy)
                     ? sqlQuery.Replace("#ORDERBY#", string.Format("{0}", orderBy))
                     : sqlQuery.Replace("#ORDERBY#", string.Format("{0}", "Id"));

            var storeReceipts = _db.Query<ReceiptViewModel>(sqlQuery, new
            {
                @storeId = storeId
            }).ToList();
            return storeReceipts;
        }
        private static string GetStoreReceiptsQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"WITH SR AS
                              (
                                    SELECT ROW_NUMBER()OVER(ORDER BY #ORDERBY#) as RowNum,Id,ReceiptDate,Amount,Vat1,Vat2,StoreId,UserId,(SELECT COUNT(Id) FROM ReceiptItem WHERE ReceiptItem.ReceiptId=Receipt.Id) as TotalArticles,
                                    COUNT(1)OVER() as ResultCount
                                    FROM Receipt WHERE StoreId=@storeId
                              )
                              SELECT r.*,U.Email FROM SR r 
                              INNER JOIN [User] U
                              ON U.Id=r.UserId
                              #WHERECONDITION#");
            return sqlQuery.ToString();
        }
        public ReceiptHistoryDetailModel GetReceiptHistoryDetail(long receiptId)
        {
            var sqlQuery = GetReceiptHistoryDetailQuery();
            var receiptHistoryModel = new ReceiptHistoryDetailModel();
            using (var multi = _db.QueryMultipleAsync(sqlQuery, new
            {
                @receiptId = receiptId
            }).Result)
            {
                receiptHistoryModel.ReceiptItems = new List<ReceiptHistoryItems>();
                receiptHistoryModel = multi.ReadAsync<ReceiptHistoryDetailModel>().Result.FirstOrDefault();
                receiptHistoryModel.ReceiptItems = multi.ReadAsync<ReceiptHistoryItems>().Result.ToList();
            }
            return receiptHistoryModel;
        }
        private static string GetReceiptHistoryDetailQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT R.Id as ReceiptId,COUNT(RI.Id) as Quantity,T.PaymentId as TransactionId,R.Vat1,R.Vat2,R.ReceiptDate as Date,SUM(R.Amount + Vat1 + Vat2) AS Summary,RI.Amount,RI.InventoryId,S.Name as Shop,I.Name as Inventory
                              FROM Receipt R
                              LEFT JOIN ReceiptItem RI
                              ON RI.ReceiptId=R.Id
                              LEFT JOIN Store S
                              ON S.Id=R.StoreId
                              LEFT JOIN Inventory I
                              ON I.Id=RI.InventoryId
                              LEFT JOIN [Transaction] T
                              ON T.ReceiptId=R.Id
                              WHERE R.Id=@receiptId 
                              GROUP BY R.Id,T.PaymentId,R.Vat1,R.Vat2,R.ReceiptDate,R.Amount,RI.Amount,RI.InventoryId,S.Name,I.Name;");

            sqlQuery.Append(@"SELECT R.Id,COUNT(RI.Id) as Quantity,RI.Amount,RI.InventoryId,I.Name as Inventory
                              FROM ReceiptItem RI
                              LEFT JOIN Receipt R
                              ON R.Id=RI.ReceiptId
                              LEFT JOIN Inventory I 
                              ON I.Id=RI.InventoryId
                              WHERE RI.ReceiptId=@receiptId
                              GROUP BY R.Id,RI.Amount,RI.InventoryId,I.Name,RI.Amount;");
            return sqlQuery.ToString();
        }
        public bool IsReceiptExist(long receiptId, long userId)
        {
            var sqlQuery = IsReceiptExistQuery();
            var isReceiptExist = _db.ExecuteScalar<int>(sqlQuery, new
            {
                @receiptId = receiptId,
                @userId = userId
            });
            return isReceiptExist > 0;
        }
        private static string IsReceiptExistQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT COUNT(Id) FROM Receipt WHERE Id=@receiptId AND UserId=@userId;");
            return sqlQuery.ToString();
        }
        public string GetTransactionIdByReceiptId(long receiptId)
        {
            var sqlQuery = GetTransactionIdByReceiptIdQuery();
            var transactionId = _db.Query<string>(sqlQuery, new
            {
                @receiptId = receiptId
            }).FirstOrDefault();
            return transactionId;
        }
        private static string GetTransactionIdByReceiptIdQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT PaymentId FROM [Transaction] WHERE ReceiptId=@receiptId;");
            return sqlQuery.ToString();
        }
        public Receipt GetReceiptById(long receiptId)
        {
            var sqlQuery = GetReceiptByIdQuery();
            var receiptObj = _db.Query<Receipt>(sqlQuery, new
            {
                @receiptId = receiptId
            }).FirstOrDefault();
            return receiptObj;
        }
        private static string GetReceiptByIdQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM Receipt WHERE Id=@receiptId;");
            return sqlQuery.ToString();
        }
    }
}
