using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.DataModels
{
    public class ReceiptItem
    {
        public long Id { get; set; }
        public long ReceiptId { get; set; }
        public long InventoryId { get; set; }
        public long Qty { get; set; }
        public Decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
    }
}
