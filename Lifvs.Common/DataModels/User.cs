using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.DataModels
{
    public class User
    {
        public virtual long Id { get; set; }
        public virtual string Email { get; set; }
        public virtual string Password { get; set; }
        public virtual string PhotoUrl { get; set; }
        public virtual string StreetAddress { get; set; }
        public virtual string PostalAddress { get; set; }
        public virtual string AreaAddress { get; set; }
        public virtual long? CountryId { get; set; }
        public virtual string AuthType { get; set; }
        public virtual string AuthId { get; set; }
        public virtual string DeviceType { get; set; }
        public virtual string DeviceId { get; set; }
        public virtual bool Active { get; set; }
        public virtual bool IsVerified { get; set; }
        public virtual DateTime? CreatedDate { get; set; }
        public virtual DateTime? ModifiedDate { get; set; }
        public virtual string UserCode { get; set; }
    }
}
