using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.DataModels
{
    public class Cart
    {
        public long Id { get; set; }
        public DateTime CartGeneratedDate { get; set; }
        public decimal Amount { get; set; }
        public decimal Vat1 { get; set; }
        public decimal Vat2 { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public bool PaymentDone { get; set; }
        public long UserId { get; set; }
        public long StoreId { get; set; }
    }
}
