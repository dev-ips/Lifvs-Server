using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.ApiModels
{
    public class ReceiptModel
    {
        public long Id { get; set; }
        public DateTime ReceiptDate { get; set; }
        public Decimal Amount { get; set; }
        public Decimal Vat1 { get; set; }
        public Decimal Vat2 { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public bool PaymentDone { get; set; }
        public long UserId { get; set; }
        public long StoreId { get; set; }
    }

    public class PurchasedHistory
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public long StoreId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string Store { get; set; }
        public long TotalPurchasedItem { get; set; }
        
    }
    public class ReceiptHistoryDetailModel
    {
        public long ReceiptId { get; set; }
        public DateTime Date { get; set; }
        public string Shop { get; set; }
        public decimal? Summary { get; set; }
        public string TransactionId { get; set; }
        public decimal? Vat1 { get; set; }
        public decimal? Vat2 { get; set; }
        public List<ReceiptHistoryItems> ReceiptItems { get; set; }
    }
    public class ReceiptHistoryItems
    {
        public string Inventory { get; set; }
        public int Quantity { get; set; }
        public decimal? Amount { get; set; }
    }
}
