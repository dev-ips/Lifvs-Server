using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Repositories.Interfaces
{
    public interface IReceiptRepository
    {
        long AddReceiptItems(string userId, string storeId, string cartId);
        List<ReceiptViewModel> GetStoreReceipts(int storeId, string whereCondition, string orderBy);
        UserCardDetailModel GetUserCardDetails(string userId);
        void UpdateUserCreditCardId(string creditCardId, string userId);
        decimal GetCartAmount(string cartId);
        string CreateTransaction(string userId, string paymentStatus, string paymentId, string paymentObject, string receiptId);
        List<ReceiptItemModel> GetAllReceiptItems(string receiptId);
        List<PurchasedHistory> GetPurchasedHistory(string userId);
        ReceiptHistoryDetailModel GetReceiptHistoryDetail(long receiptId);
        bool IsReceiptExist(long receiptId, long userId);
        string GetTransactionIdByReceiptId(long receiptId);
        Receipt GetReceiptById(long receiptId);
        List<ExpiredCardModel> CheckAllExpiredCarts();
    }
}
