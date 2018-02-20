using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.DataModels
{
   public class StoreInventory
    {
        public long Id { get; set; }
        public long StoreId { get; set; }
        public long InventoryId { get; set; }
        public long? NumberOfItems { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
