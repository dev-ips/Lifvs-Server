using Lifvs.Common.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Services.Interfaces
{
    public interface IReceiptService
    {
        string Payment(string userId, string storeId, string cartId);
        List<ReceiptViewModel> GetStoreReceipts(int storeId, int offSet, int rows, string filters, string sidx, string sord);
        List<PurchasedHistory> GetPurchasedHistory(string userId);
        ReceiptHistoryDetailModel GetReceiptHistoryDetail(long userId, long receiptId);
        bool SendReceipt(long receiptId, long userId);
        void CheckAllExpiredCarts();
    }
}
