using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.DataModels
{
    public class Transaction
    {
        public long Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public long UserId { get; set; }
        public string Status { get; set; }
        public string PaymentId { get; set; }
        public string PaymentObject { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public long ReceiptId { get; set; }
    }
}
