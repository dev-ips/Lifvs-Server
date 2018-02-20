using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.ApiModels
{
    public class CartItemModel
    {
        public long Id { get; set; }
        public long CartId { get; set; }
        public long InventoryId { get; set; }
        public long Quantity { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }

        public string InventoryName { get; set; }
    }

    public class OfflineCartItemModel
    {
        public Int64[] InventoryIds { get; set; }
    }

    public class AddCartItemResponeseModel
    {
        public Int64 InventoryId { get; set; }
        public Int64 CartItemId { get; set; }
    }
    public class TotalQuantityByCart
    {
        public long CartId { get; set; }
        public long InventoryId { get; set; }
        public long TotalQuantity { get; set; }
    }

    public class ExpiredCardModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long StoreId { get; set; }
        public DateTime CartGeneratedDate { get; set; }
    }
}
