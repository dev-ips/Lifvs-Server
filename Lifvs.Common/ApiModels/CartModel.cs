using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.ApiModels
{
    public class CartModel
    {
        public long Id { get; set; }
        public DateTime CartGeneratedDate { get; set; }
        public long UserId { get; set; }
        public long StoreId { get; set; }
        public bool PaymentDone { get; set; }
    }
}
