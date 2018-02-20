using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.DataModels
{
    public class CartItem
    {
        public long Id { get; set; }
        public long CartId { get; set; }
        public long InventoryId { get; set; }
        public long Quantity { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
    }
}
