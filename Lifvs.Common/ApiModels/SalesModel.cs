using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.ApiModels
{
    public class SalesModel
    {
        public long? StoreId { get; set; }
        public string StoreName { get; set; }
    }
    public class ReceiptViewModel
    {
        public long Id { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Vat1 { get; set; }
        public decimal? Vat2 { get; set; }
        public long? StoreId { get; set; }
        public long? UserId { get; set; }
        public string Email { get; set; }
        public int TotalArticles { get; set; }
        public int ResultCount { get; set; }
        public int RowNum { get; set; }
    }
}
