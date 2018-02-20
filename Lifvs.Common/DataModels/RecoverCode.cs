using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.DataModels
{
    public class RecoverCode
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public string RecoveryCode { get; set; }
        public bool Active { get; set; }
        public DateTime ExpiredOn { get; set; }
    }
}
