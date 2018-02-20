using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.ApiModels
{
    public class UserCodeModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long StoreId { get; set; }
        public string Code { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class GetUserCodeModel
    {
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public string ErrorMessage { get; set; }

    }
}
