using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.DataModels
{
    public class Inventory
    {
        public long Id { get; set; }
        public string BrandName { get; set; }
        public string Name { get; set; }
        public string InventoryCode { get; set; }
        public string ImagePath { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string VolumeType { get; set; }
        public string Supplier { get; set; }
        public decimal InPrice { get; set; }
        public decimal OutPriceIncVat { get; set; }
        public decimal REA { get; set; }
        public string Volume { get; set; }
        public string Specification { get; set; }
    }
}
